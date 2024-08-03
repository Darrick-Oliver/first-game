using Godot;

public partial class Car : CharacterBody2D
{
	[Export]
	private int MaxSpeed { get; set; } = 800;
	[Export]
	private float FwdAcceleration { get; set; } = 30f;
	[Export]
	private float RevAcceleration { get; set; } = 15f;
	[Export]
	private float FrictionCoefficient { get; set; } = 7.5f;
	[Export]
	private int Radius { get; set; } = 10;

	private float _rotationDirection;
	private float _thrustDirection;

	private AnimatedSprite2D _animatedSprite;
	private GpuParticles2D _particles;

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_particles = GetNode<GpuParticles2D>("GPUParticles2D");
		_particles.Emitting = false;
	}

	private void GetInput()
	{
		_rotationDirection = Input.GetAxis("move_left", "move_right");
		_thrustDirection = Input.GetAxis("move_down", "move_up");
	}

	private void SetVelocity()
	{
		// Thrust (applied by user)
		var thrustAccelerationMagnitude = _thrustDirection > 0 ? FwdAcceleration : RevAcceleration;
		var thrust = -1 * Transform.Y * _thrustDirection * thrustAccelerationMagnitude;

		// External forces
		var frictionAcceleration = Velocity.Normalized() * -1 * FrictionCoefficient;

		// Determine acceleration
		var acceleration = thrust + frictionAcceleration;
		Velocity += acceleration;

		if (Velocity.Length() < FrictionCoefficient && thrust.Length() == 0) {
			Velocity = Vector2.Zero;
		}

		if (Velocity.Length() > MaxSpeed) {
			Velocity = Velocity.Normalized() * MaxSpeed;
		}
	}

	private void SetRotation(double delta)
	{
		var angVelocityMagnitude = Velocity.Length() / Radius;
		var rotationalAcceleration = _rotationDirection * angVelocityMagnitude * (float)delta;

		if (_thrustDirection != 0) {
			rotationalAcceleration *= _thrustDirection;
		}

		Rotation += rotationalAcceleration;
	}

	private void PlayAnimation()
	{
		if (_thrustDirection > 0) {
			_particles.Emitting = true;
		} else {
			_particles.Emitting = false;
		}

		if (_rotationDirection > 0) {
			_animatedSprite.Play("right_turn");
		} else if (_rotationDirection < 0) {
			_animatedSprite.Play("left_turn");
		} else if (_thrustDirection > 0) {
			_animatedSprite.Play("straight");
		} else {
			_animatedSprite.Play("straight");
			_animatedSprite.Stop();
		}
	}

	public override void _Process(double delta)
	{
		GetInput();
		
		SetVelocity();
		SetRotation(delta);
		PlayAnimation();

		MoveAndSlide();
	}
}
