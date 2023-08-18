using System.Diagnostics;
using System.Security.Principal;

void RelaunchIfNotAdmin() {
    if (!RunningAsAdmin()) {
        Console.WriteLine("Running as admin required!");
        ProcessStartInfo proc = new ProcessStartInfo();
        proc.UseShellExecute = true;
        proc.WorkingDirectory = Environment.CurrentDirectory;
        proc.FileName = "CuttermanKiller.exe";
        proc.Verb = "runas";
        try {
            Process.Start(proc);
            Environment.Exit(0);
        }
        catch (Exception) {
            Console.WriteLine("请右键以管理员身份运行！");
            Environment.Exit(0);
        }
    }
}

bool RunningAsAdmin() {
    WindowsIdentity id = WindowsIdentity.GetCurrent();
    WindowsPrincipal principal = new WindowsPrincipal(id);

    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

bool hasPatched;

void ReplacePatched(ref string js, string id, string old, string now) {
    if (js.Contains(old)) {
        js = js.Replace(old, now);
        Console.WriteLine($"[{id}]Patched*{old}*->*{now}*完成");
        hasPatched = true;
    }
    else {
        Console.WriteLine($"[{id}]Patched*{old}*没有点位");
    }
}

const string com_cutterman_cleaner = "com.cutterman.cleaner";
const string com_cutterman_cutterman_panel = "com.cutterman.cutterman.panel";
const string com_cutterman_parker_panel = "com.cutterman.parker.panel";
const string com_cutterman_layer_panel = "com.cutterman.layer.panel";
const string GuideMe = "GuideMe";

async Task PatchedPlugins(params string[] dirs) {
    foreach (var dir in dirs) {
        Console.WriteLine($"开始查找插件{dir}");
        var panelJs = @$"C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\{dir}\panel.js";
        if (!File.Exists(panelJs)) {
            panelJs = @$"C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\{dir}\panel\panel.js";
            if (!File.Exists(panelJs)) {
                panelJs = @$"C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\{dir}\Panel\js\All.js";
                if (!File.Exists(panelJs)) {
                    Console.WriteLine($"跳过未安装插件{dir}");
                    continue;
                }
            }
        }

        var js = await File.ReadAllTextAsync(panelJs);
        hasPatched = false;
        switch (dir) {
            case com_cutterman_cutterman_panel:
                ReplacePatched(ref js, com_cutterman_cutterman_panel, "panelState:Om.Login", "panelState:Om.Preview");
                break;
            case com_cutterman_cleaner:
                ReplacePatched(ref js, com_cutterman_cleaner, "panelState:Xc.Login", "panelState:Xc.Main");
                ReplacePatched(ref js, com_cutterman_cleaner, "user:this.state.user", "user:\"\"");
                break;
            case com_cutterman_parker_panel:
                ReplacePatched(ref js, com_cutterman_parker_panel, "panelState:Zg.Login", "panelState:Zg.Preview");
                break;
            case com_cutterman_layer_panel:
                ReplacePatched(ref js, com_cutterman_layer_panel, "panelState:Fg.login", "panelState:Fg.index");
                break;
            case GuideMe:
                ReplacePatched(ref js, GuideMe, "_this.isLogin = false;", "_this.isLogin = true;");
                break;
        }

        if (hasPatched) {
            await File.WriteAllTextAsync(panelJs, js);
            Console.WriteLine($"写入{panelJs}文件完成");
        }
    }
}


RelaunchIfNotAdmin();
Console.WriteLine("CuttermanKiller v1.0 by binaryinject");
await PatchedPlugins(com_cutterman_cutterman_panel, com_cutterman_cleaner, com_cutterman_parker_panel,
    com_cutterman_layer_panel, GuideMe);
Console.WriteLine("完成全部任务任意键关闭");
Console.Read();