package main

import (
	"context"
	"crypto/tls"
	"errors"
	"fmt"
	"io"
	"log"
	"net"
	"strings"
	"sync"
	"time"

	pb "ech-client/proto"

	"github.com/gorilla/websocket"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/metadata"
)

// ======================== Transport 接口定义 ========================

// TunnelConn 表示一个隧道连接（抽象接口）
type TunnelConn interface {
	// Connect 发送连接请求并等待响应
	Connect(target string, initialData []byte) error
	// Read 从隧道读取数据
	Read() ([]byte, error)
	// Write 向隧道写入数据
	Write(data []byte) error
	// Close 关闭连接
	Close() error
	// StartPing 启动心跳（返回停止通道）
	StartPing(interval time.Duration) chan struct{}
}

// Transport 传输层接口
type Transport interface {
	// Dial 建立新连接
	Dial() (TunnelConn, error)
	// Name 返回传输层名称
	Name() string
}

// ======================== WebSocket Transport ========================

type WebSocketTransport struct {
	serverAddr string
	serverIP   string
	token      string
	useTLS     bool
	useECH     bool
}

func NewWebSocketTransport(serverAddr, serverIP, token string, useECH bool) *WebSocketTransport {
	return &WebSocketTransport{
		serverAddr: serverAddr,
		serverIP:   serverIP,
		token:      token,
		useTLS:     true,
		useECH:     useECH,
	}
}

func (t *WebSocketTransport) Name() string {
	if t.useECH {
		return "WebSocket+ECH"
	}
	return "WebSocket+TLS"
}

func (t *WebSocketTransport) Dial() (TunnelConn, error) {
	host, port, path, err := parseServerAddr(t.serverAddr)
	if err != nil {
		return nil, err
	}

	wsURL := fmt.Sprintf("wss://%s:%s%s", host, port, path)

	var tlsCfg *tls.Config

	if t.useECH && !fallback {
		echBytes, echErr := getECHList()
		if echErr != nil {
			return nil, echErr
		}
		tlsCfg, err = buildTLSConfigWithECH(host, echBytes)
		if err != nil {
			return nil, err
		}
	} else {
		tlsCfg = &tls.Config{
			ServerName: host,
			MinVersion: tls.VersionTLS13,
		}
	}

	dialer := websocket.Dialer{
		TLSClientConfig: tlsCfg,
		Subprotocols: func() []string {
			if t.token == "" {
				return nil
			}
			return []string{t.token}
		}(),
		HandshakeTimeout: 10 * time.Second,
	}

	if t.serverIP != "" {
		dialer.NetDial = func(network, address string) (net.Conn, error) {
			_, p, err := net.SplitHostPort(address)
			if err != nil {
				return nil, err
			}
			return net.DialTimeout(network, net.JoinHostPort(t.serverIP, p), 10*time.Second)
		}
	}

	wsConn, _, err := dialer.Dial(wsURL, nil)
	if err != nil {
		return nil, err
	}

	return &WebSocketConn{conn: wsConn}, nil
}

// WebSocketConn WebSocket 连接实现
type WebSocketConn struct {
	conn *websocket.Conn
	mu   sync.Mutex
}

func (c *WebSocketConn) Connect(target string, initialData []byte) error {
	connectMsg := fmt.Sprintf("CONNECT:%s|%s", target, string(initialData))
	c.mu.Lock()
	err := c.conn.WriteMessage(websocket.TextMessage, []byte(connectMsg))
	c.mu.Unlock()
	if err != nil {
		return err
	}

	_, msg, err := c.conn.ReadMessage()
	if err != nil {
		return err
	}

	response := string(msg)
	if strings.HasPrefix(response, "ERROR:") {
		return errors.New(response)
	}
	if response != "CONNECTED" {
		return fmt.Errorf("unexpected response: %s", response)
	}

	return nil
}

func (c *WebSocketConn) Read() ([]byte, error) {
	mt, msg, err := c.conn.ReadMessage()
	if err != nil {
		return nil, err
	}

	if mt == websocket.TextMessage && string(msg) == "CLOSE" {
		return nil, io.EOF
	}

	return msg, nil
}

func (c *WebSocketConn) Write(data []byte) error {
	c.mu.Lock()
	defer c.mu.Unlock()
	return c.conn.WriteMessage(websocket.BinaryMessage, data)
}

func (c *WebSocketConn) Close() error {
	c.mu.Lock()
	c.conn.WriteMessage(websocket.TextMessage, []byte("CLOSE"))
	c.mu.Unlock()
	return c.conn.Close()
}

func (c *WebSocketConn) StartPing(interval time.Duration) chan struct{} {
	stopChan := make(chan struct{})
	go func() {
		ticker := time.NewTicker(interval)
		defer ticker.Stop()
		for {
			select {
			case <-ticker.C:
				c.mu.Lock()
				c.conn.WriteMessage(websocket.PingMessage, nil)
				c.mu.Unlock()
			case <-stopChan:
				return
			}
		}
	}()
	return stopChan
}

// ======================== gRPC Transport ========================

type GRPCTransport struct {
	serverAddr string
	serverIP   string
	uuid       string
	useTLS     bool
	useECH     bool
}

func NewGRPCTransport(serverAddr, serverIP, uuid string, useTLS, useECH bool) *GRPCTransport {
	return &GRPCTransport{
		serverAddr: serverAddr,
		serverIP:   serverIP,
		uuid:       uuid,
		useTLS:     useTLS,
		useECH:     useECH,
	}
}

func (t *GRPCTransport) Name() string {
	if t.useECH {
		return "gRPC+ECH"
	}
	if t.useTLS {
		return "gRPC+TLS"
	}
	return "gRPC"
}

func (t *GRPCTransport) Dial() (TunnelConn, error) {
	host, port, _, err := parseServerAddr(t.serverAddr)
	if err != nil {
		return nil, err
	}

	// 确定连接地址
	addr := net.JoinHostPort(host, port)
	if t.serverIP != "" {
		addr = net.JoinHostPort(t.serverIP, port)
	}

	var opts []grpc.DialOption

	if t.useTLS || t.useECH {
		var tlsCfg *tls.Config

		if t.useECH {
			// 使用 ECH + TLS 1.3
			echBytes, echErr := getECHList()
			if echErr != nil {
				return nil, fmt.Errorf("获取 ECH 配置失败: %w", echErr)
			}
			tlsCfg, err = buildTLSConfigWithECH(host, echBytes)
			if err != nil {
				return nil, fmt.Errorf("构建 ECH TLS 配置失败: %w", err)
			}
			log.Printf("[gRPC] 使用 ECH + TLS 1.3 连接")
		} else {
			// 普通 TLS
			tlsCfg = &tls.Config{
				ServerName: host,
				MinVersion: tls.VersionTLS13,
			}
		}

		opts = append(opts, grpc.WithTransportCredentials(credentials.NewTLS(tlsCfg)))
	} else {
		opts = append(opts, grpc.WithTransportCredentials(insecure.NewCredentials()))
	}

	// 连接超时
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	conn, err := grpc.DialContext(ctx, addr, opts...)
	if err != nil {
		return nil, fmt.Errorf("gRPC dial failed: %w", err)
	}

	client := pb.NewProxyServiceClient(conn)

	// 创建带 metadata 的 context（用于鉴权）
	md := metadata.New(map[string]string{"uuid": t.uuid})
	streamCtx := metadata.NewOutgoingContext(context.Background(), md)

	stream, err := client.Tunnel(streamCtx)
	if err != nil {
		conn.Close()
		return nil, fmt.Errorf("gRPC stream failed: %w", err)
	}

	return &GRPCConn{
		conn:   conn,
		stream: stream,
	}, nil
}

// GRPCConn gRPC 连接实现
type GRPCConn struct {
	conn   *grpc.ClientConn
	stream pb.ProxyService_TunnelClient
	mu     sync.Mutex
}

func (c *GRPCConn) Connect(target string, initialData []byte) error {
	// 构建 CONNECT 消息（与 WebSocket 协议兼容）
	connectMsg := fmt.Sprintf("CONNECT:%s|", target)
	data := append([]byte(connectMsg), initialData...)

	c.mu.Lock()
	err := c.stream.Send(&pb.SocketData{Content: data})
	c.mu.Unlock()
	if err != nil {
		return err
	}

	// 等待响应
	resp, err := c.stream.Recv()
	if err != nil {
		return err
	}

	response := string(resp.Content)
	if strings.HasPrefix(response, "ERROR:") {
		return errors.New(response)
	}
	if response != "CONNECTED" {
		return fmt.Errorf("unexpected response: %s", response)
	}

	return nil
}

func (c *GRPCConn) Read() ([]byte, error) {
	resp, err := c.stream.Recv()
	if err != nil {
		return nil, err
	}
	return resp.Content, nil
}

func (c *GRPCConn) Write(data []byte) error {
	c.mu.Lock()
	defer c.mu.Unlock()
	return c.stream.Send(&pb.SocketData{Content: data})
}

func (c *GRPCConn) Close() error {
	if c.stream != nil {
		c.stream.CloseSend()
	}
	if c.conn != nil {
		return c.conn.Close()
	}
	return nil
}

func (c *GRPCConn) StartPing(interval time.Duration) chan struct{} {
	// gRPC 有内置的 keepalive，这里返回空的 stop channel
	stopChan := make(chan struct{})
	// gRPC 不需要应用层心跳
	return stopChan
}

// ======================== Transport 工厂函数 ========================

// 传输模式常量
const (
	TransportWebSocket = "ws"
	TransportGRPC      = "grpc"
)

var (
	currentTransport Transport
	transportMode    string
)

// InitTransport 初始化传输层
func InitTransport(mode, serverAddr, serverIP, token string, useECH bool) {
	transportMode = mode

	switch mode {
	case TransportGRPC:
		// gRPC 模式：token 作为 UUID，支持 ECH
		useTLS := !strings.HasPrefix(serverAddr, "grpc://")
		addr := strings.TrimPrefix(strings.TrimPrefix(serverAddr, "grpcs://"), "grpc://")
		currentTransport = NewGRPCTransport(addr, serverIP, token, useTLS, useECH)
		log.Printf("[传输层] 使用 gRPC 模式: %s (ECH: %v)", addr, useECH)

	default:
		// 默认 WebSocket 模式
		currentTransport = NewWebSocketTransport(serverAddr, serverIP, token, useECH)
		log.Printf("[传输层] 使用 WebSocket 模式: %s (ECH: %v)", serverAddr, useECH)
	}
}

// GetTransport 获取当前传输层
func GetTransport() Transport {
	return currentTransport
}

// DialTunnel 建立隧道连接（使用当前传输层）
func DialTunnel() (TunnelConn, error) {
	if currentTransport == nil {
		return nil, errors.New("transport not initialized")
	}
	return currentTransport.Dial()
}
