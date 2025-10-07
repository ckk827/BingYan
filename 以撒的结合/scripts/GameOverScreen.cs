using Godot;

public partial class GameOverScreen : CanvasLayer
{
    [Export] private Label resultLabel;
    [Export] private Button restartButton;

    public override void _Ready()
    {

        GD.Print("Current Scene is: ", GetTree().CurrentScene?.Name);

        if (resultLabel == null)
        {
            GD.PrintErr("ResultLabel not assigned in editor!");
        }

        if (restartButton == null)
        {
            GD.PrintErr("RestartButton not assigned in editor!");
        }

  
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

 
        GetTree().Paused = true;
    }

    private void OnRestartPressed()
    {
       
        GetTree().Paused = false;
        // 重新加载当前场景
        GetTree().ReloadCurrentScene();
    }
}