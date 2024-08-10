using System;
using Godot;

public partial class Car : CharacterBody2D
{
  [Export]
  private int MaxSpeed { get; set; } = 800;
  [Export]
  private float FwdAcceleration { get; set; } = 30f;
  [Export]
  private float RevAcceleration { get; set; } = 20f;
  [Export]
  private float FrictionCoefficient { get; set; } = 14f;
  [Export]
  private int Radius { get; set; } = 300;
  
  [Export]
  private Marker2D[] SkidMarkers;
  [Export]
  private GpuParticles2D[] SkidParticles;

  private float _rotationDirection;
  private float _thrustDirection;

  private AnimatedSprite2D _animatedSprite;
  private PackedScene _skidScene;


  private bool _wasSkidding = false;

  public override void _Ready()
  {
    _skidScene = GD.Load<PackedScene>("res://src/components/skid/skid.tscn");
    _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
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

  private bool IsSkidding()
  {
    if (Math.Abs(Transform.X.Dot(Velocity)) > 0.7 * MaxSpeed) {
      return true;
    }

    return false;
  }

  private void StartSkid(Skid instance)
  {
    if (instance.Points.Length == 0) {
      foreach (var particle in SkidParticles) {
        particle.Emitting = true;
      }
    }
    instance.AddPoint();
  }

  private void StopSkid(Skid instance) {
    foreach (var particle in SkidParticles) {
      particle.Emitting = false;
    }
    instance.Fade();
  }

  private void Skid()
  {
    if (IsSkidding()) {
      foreach (var marker in SkidMarkers) {
        Skid skidInstance;
        if (_wasSkidding) {
          skidInstance = marker.GetChild<Skid>(-1);
        } else {
          skidInstance = _skidScene.Instantiate<Skid>();
          marker.AddChild(skidInstance);
        }
        StartSkid(skidInstance);
      }
      _wasSkidding = true;
    } else if (_wasSkidding) {
      _wasSkidding = false;
      foreach (var marker in SkidMarkers) {
        var skidInstance = marker.GetChild<Skid>(-1);
        StopSkid(skidInstance);
      }
    }
  }

  public override void _Process(double delta)
  {
    // Input
    GetInput();
    
    // Physics
    SetVelocity();
    SetRotation(delta);
    MoveAndSlide();

    // Effects
    PlayAnimation();
    Skid();
  }
}
