namespace AntDesign.X.Blazor.Demo.Services;

/// <summary>
/// 极简 .env 加载器。仅在 Demo 进程启动时读取项目根的 .env，将变量
/// 注入当前进程环境（不写入磁盘、不入库），供 IConfiguration 读取。
/// </summary>
public static class DotEnvLoader
{
    public static void Load(string? filePath = null)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            candidates.Add(filePath!);
        }

        // 当前工作目录
        candidates.Add(Path.Combine(Environment.CurrentDirectory, ".env"));

        // 向上查找仓库根（最多 6 层）
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 6 && dir is not null; i++)
        {
            candidates.Add(Path.Combine(dir, ".env"));
            dir = Path.GetDirectoryName(dir);
        }

        foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#'))
                {
                    continue;
                }

                var idx = line.IndexOf('=');
                if (idx <= 0)
                {
                    continue;
                }

                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();
                if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                {
                    value = value[1..^1];
                }

                if (Environment.GetEnvironmentVariable(key) is null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }

            return;
        }
    }
}
