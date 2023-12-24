using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Objects.Common.Bridge;

using Player;

public partial class Bridge(Texture2D logTexture, byte logAmount, int logSize) : CommonObject
{
    private int _activeLogId;
    private int _maxDepression;
    private float _angle;
    private int _width;

    private Vector2[] _logPositions;
    private int[] _depression;
    private int _logSizeHalf;

    public override void _Ready()
    {
        _width = logAmount * logSize;
        _logPositions = new Vector2[logAmount];
        _depression = new int[logAmount];
        
        _logSizeHalf = logSize / 2;
        for (var i = 0; i < logAmount; i++)
        {
            _logPositions[i] = -new Vector2(logAmount * _logSizeHalf + logSize * i + _logSizeHalf, 0f);
            _depression[i] = (i < logAmount / 2 ? i + 1 : logAmount - i) * 2;
        }

        // Player should not balance on this object
        SolidData.NoBalance = true;

        // Properties
        //TODO: depth
        SetSolid(new Vector2I(logAmount * _logSizeHalf, _logSizeHalf));
        SetBehaviour(BehaviourType.Pause);
    }

    public override void _Draw()
    {
        for (var i = 0; i < logAmount; i += logSize)
        {
            DrawTexture(logTexture, _logPositions[i]);
        }
    }

    protected override void Update(float processSpeed)
    {
        var maxDepression = 0;
		var isPlayerTouch = false;
		
		foreach (Player player in Player.Players)
		{
		    ActSolid(player, Constants.SolidType.Top);
		    
			if (!CheckCollision(player, Constants.CollisionSensor.SolidU)) continue;
			
			isPlayerTouch = true;
			
			int activeLogId = Math.Clamp(
				((int)(player.Position.X - Position.X) + logAmount * _logSizeHalf) / logSize + 1, 1, logAmount);
				
			int depressionValue = _depression[activeLogId - 1];
			if (depressionValue > maxDepression)
			{
				_activeLogId = activeLogId;
				_maxDepression = depressionValue;
				
				// Remember current dip value for the next player
				maxDepression = _maxDepression;
			}
			
			player.Position += new Vector2(0f, 
				MathF.Round(depressionValue * MathF.Sin(Mathf.DegToRad(_angle))) + 1);
		}

		UpdateLogPositions();

		UpdateAngle(isPlayerTouch, processSpeed);
		
		QueueRedraw();
    }

    private void UpdateLogPositions()
    {
	    for (var i = 0; i < logAmount; i++)
	    {
		    int logDifference = Math.Abs(i - _activeLogId + 1);
		    var logDistance = 1;
			
		    if (i < _activeLogId)
		    {
			    logDistance -= logDifference / _activeLogId;
		    }
		    else
		    {
			    logDistance -= logDifference / (logAmount - _activeLogId + 1);
		    }
			
		    _logPositions[i].Y = MathF.Round(
			    _maxDepression * MathF.Sin(Mathf.DegToRad(90 * logDistance) * MathF.Sin(Mathf.DegToRad(_angle))));
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
