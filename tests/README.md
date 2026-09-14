# 验证记录

CoreTests：计时截止时间、暂停保留、恢复、到期单次触发、重置、中文任务保存、设置保存、损坏备份恢复均通过。

UiTests：真实 WinForms 窗口的开始/暂停/重置、实时倒计时、任务添加/完成/删除、日期隔离/历史保留、三个模式、设置修改与保存、到期弹窗单次触发、准备休息不自动开始、完成文字保留、置顶通过。

`screenshots/focus.png`、`rest.png`、`compact.png` 为自动验收产生的实际窗口图，已目视检查。

测试使用 `tests/test-data` 和 `tests/ui-data` 内的独立数据，不读写日常任务数据。

编译和运行测试（PowerShell）：

```powershell
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:library /out:tests\Core.dll src\Core.cs
& $compiler /nologo /out:tests\CoreTests.exe /r:Microsoft.CSharp.dll tests\CoreTests.cs
& .\tests\CoreTests.exe
& $compiler /nologo /out:tests\UiTests.exe /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:Microsoft.CSharp.dll tests\UiTests.cs
& .\tests\UiTests.exe 'dist\PigLiftClock.exe'
```

UI 测试会短暂显示窗口并自动关闭。未进行跨机器兼容性验证；已在当前 Windows 环境编译和运行。
