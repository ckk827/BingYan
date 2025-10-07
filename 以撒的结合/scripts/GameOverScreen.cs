using Godot;

public partial class GameOverScreen : CanvasLayer
{
    [Export] private Label resultLabel;
    [Export] private Button restartButton;

    public override void _Ready()
    {
        // 确保在编辑器中正确分配了节点
        GD.Print("Current Scene is: ", GetTree().CurrentScene?.Name);

        if (resultLabel == null)
        {
            GD.PrintErr("ResultLabel not assigned in editor!");
        }

        if (restartButton == null)
        {
            GD.PrintErr("RestartButton not assigned in editor!");
        }

        // 初始隐藏
        Hide();
    }

    public void ShowResult(bool isWin)
    {
        if (resultLabel != null)
        {
            resultLabel.Text = isWin ? "WIN!" : "LOSE";
            resultLabel.Modulate = isWin ? Colors.Green : Colors.Red;
        }

        Show();

        // 暂停游戏
        GetTree().Paused = true;
    }

    private void OnRestartPressed()
    {
        // 取消暂停
        GetTree().Paused = false;
        // 重新加载当前场景
        GetTree().ReloadCurrentScene();
    }
}