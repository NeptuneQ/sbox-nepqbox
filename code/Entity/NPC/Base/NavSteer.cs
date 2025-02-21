﻿using Sandbox;
using Steamworks;
using System;
using System.Buffers;

public class NavSteer
{
	protected NavPath Path { get; private set; }

	public Func<Entity, bool> DontAvoidance = e => !e.EnableDrawing;

	public NavSteer()
	{
		Path = new NavPath();
	}

	public virtual void Tick( Vector3 currentPosition )
	{
		Path.Update( currentPosition, Target );

		Output.Finished = Path.IsEmpty;

		if ( Output.Finished )
		{
			Output.Direction = Vector3.Zero;
			return;
		}

		Output.Direction = Path.GetDirection( currentPosition );

		var avoid = GetAvoidance( currentPosition, 500 );
		if ( !avoid.IsNearlyZero() )
		{
			Output.Direction = (Output.Direction + avoid).Normal;
		}
	}

	Vector3 GetAvoidance( Vector3 position, float radius )
	{
		var center = position + Output.Direction * radius * 0.5f;

		var objectRadius = 200.0f;
		Vector3 avoidance = default;

		foreach ( var ent in Entity.FindInSphere( center, radius ) )
		{
			if ( ent is not NPC ) continue;
			if ( DontAvoidance.Invoke( ent ) ) continue;
			if ( ent.IsWorld ) continue;

			var delta = (position - ent.Position).WithZ( 0 );
			var closeness = delta.Length;
			if ( closeness < 0.001f ) continue;
			var thrust = ((objectRadius - closeness) / objectRadius).Clamp( 0, 1 );
			if ( thrust <= 0 ) continue;

			//avoidance += delta.Cross( Output.Direction ).Normal * thrust * 2.5f;
			avoidance += delta.Normal * thrust * thrust;
		}

		return avoidance;
	}

	public Vector3 Target { get; set; }

	public NavSteerOutput Output;


	public struct NavSteerOutput
	{
		public bool Finished;
		public Vector3 Direction;
	}
}
