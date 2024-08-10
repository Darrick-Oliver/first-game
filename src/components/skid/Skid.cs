using Godot;

public partial class Skid : Line2D
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TopLevel = true;
	}

	public void AddPoint()
	{
		AddPoint(GetParent<Marker2D>().GlobalPosition);
	}

	public void Fade()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "modulate:a", 0f, 0.5).SetDelay(5);
		tween.Connect("finished", Callable.From(() => QueueFree()));
	}
}
