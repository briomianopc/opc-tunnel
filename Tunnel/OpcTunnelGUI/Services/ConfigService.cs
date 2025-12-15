using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OpcTunnelGUI.Models;

namespace OpcTunnelGUI.Services;

public class ConfigService
{
    private readonly string _configPath;

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appData, "OpcTunnelGUI");
        Directory.CreateDirectory(configDir);
        _configPath = Path.Combine(configDir, "config.json");
    }

    public async Task<TunnelConfig?> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(_configPath);
            return JsonSerializer.Deserialize<TunnelConfig>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveConfigAsync(TunnelConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch
        {
            // 忽略保存错误
        }
    }

    public string GetConfigPath() => _configPath;
}
