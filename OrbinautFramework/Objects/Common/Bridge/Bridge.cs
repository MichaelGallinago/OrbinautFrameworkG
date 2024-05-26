using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Common.Bridge;

using Player;

public partial class Bridge(Texture2D logTexture, byte logAmount, int logSize) : BaseObject
{
    private int _activeLogId;
    private int _maxDip;
    private float _angle;
    private int _width;

    private Vector2[] _logPositions;
    private int[] _dip;
    private int _logSizeHalf;

    public override void _Ready()
    {
        _width = logAmount * logSize;
        _logPositions = new Vector2[logAmount];
        _dip = new int[logAmount];
        
        _logSizeHalf = logSize / 2;
        int halfWidth = logAmount * _logSizeHalf - _logSizeHalf;
        for (var i = 0; i < logAmount; i++)
        {
            _logPositions[i] = new Vector2(logSize * i - halfWidth, 0f);
            _dip[i] = (i < logAmount / 2 ? i + 1 : logAmount - i) * 2;
        }

        // Player should not balance on this object
        SolidData.NoBalance = true;

        // Properties
        SetSolid(new Vector2I(logAmount * _logSizeHalf, _logSizeHalf));
        Culling = CullingType.Reset;
    }
    
    public override void _Process(double delta)
    {
	    var maxDip = 0;
	    var isPlayerTouch = false;

	    foreach (Player player in Scene.Local.Players.Values)
	    {
		    player.ActSolid(this, Constants.SolidType.Top);
		    
		    if (!CheckSolidCollision(player, Constants.CollisionSensor.Top)) continue;
			
		    isPlayerTouch = true;
			
		    int activeLogId = Math.Clamp(
			    ((int)(player.Position.X - Position.X) + logAmount * _logSizeHalf) / logSize + 1, 1, logAmount);
				
		    int dip = _dip[activeLogId - 1];
		    if (dip > maxDip)
		    {
			    _activeLogId = activeLogId;
			    _maxDip = dip;
				
			    // Remember current dip value for the next player
			    maxDip = _maxDip;
		    }
			
		    player.Position += new Vector2(0f, MathF.Round(dip * MathF.Sin(Mathf.DegToRad(_angle))) + 1f);
	    }

	    UpdateLogPositions();

	    UpdateAngle(isPlayerTouch, Scene.Local.ProcessSpeed);
		
	    QueueRedraw();
    }

    public override void _Draw()
    {
        for (var i = 0; i < logAmount; i++)
        {
            DrawTexture(logTexture, _logPositions[i]);
        }
    }
    
    private void UpdateLogPositions()
    {
	    float sine = MathF.Sin(Mathf.DegToRad(_angle));
	    for (var i = 0; i < logAmount; i++)
	    {
		    float logDifference = Math.Abs(i - _activeLogId + 1);
		    float logDistance = 1f - logDifference / (i < _activeLogId ? _activeLogId : logAmount - _activeLogId + 1);
			
		    _logPositions[i].Y = -_logSizeHalf +
				MathF.Round(_maxDip * MathF.Sin(Mathf.DegToRad(90 * logDistance)) * sine);
	    }
    }

    private void UpdateAngle(bool isPlayerTouch, float processSpeed)
    {
	    const float angleChangeSpeed = 5.625f;
	    
	    if (isPlayerTouch)
	    {
		    if (_angle >= 90f) return;
			_angle += angleChangeSpeed * processSpeed;
		    return;
	    }

	    if (_angle <= 0f) return;
		_angle -= angleChangeSpeed * processSpeed;
    }
}
