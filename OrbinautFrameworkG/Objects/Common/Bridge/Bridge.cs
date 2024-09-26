using System;
using Godot;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Common.Bridge;

public partial class Bridge : SolidNode
{
	[Export] private BridgeEditor _editor;
	
	private Texture2D _logTexture;
	private byte _logAmount;
	private byte _logWidth;
	private short _logHalfWidth;
	
	private int _width;
	private int _maxDip;
	private float _angle;
    private int _activeLogId;

    private Vector2[] _logPositions;
    private int[] _dip;

    public override void _Ready()
    {
	    _logAmount = _editor.LogAmount;
	    _logWidth = _editor.LogWidth;
	    _logTexture = _editor.LogTexture;
	    
	    _editor.QueueFree();
	    
	    _width = _logWidth * _logAmount;
	    _logHalfWidth = (byte)(_logWidth / 2);
	    _logPositions = new Vector2[_logAmount];
	    _dip = new int[_logAmount];
	    
	    int halfWidth = _logAmount * _logHalfWidth - _logHalfWidth;
	    for (var i = 0; i < _logAmount; i++)
	    {
		    _logPositions[i] = new Vector2(_logWidth * i - halfWidth, 0f);
		    _dip[i] = (i < _logAmount / 2 ? i + 1 : _logAmount - i) * 2;
	    }
        
	    SolidBox.Set(_logAmount * _logHalfWidth, _logHalfWidth);
    }

    public override void _Process(double delta)
    {
	    var maxDip = 0;
	    var isPlayerTouch = false;

	    foreach (IPlayer player in Scene.Instance.Players.Values)
	    {
		    player.ActSolid(this, Constants.SolidType.Top);
		    
		    if (!player.CheckSolidCollision(SolidBox, Constants.CollisionSensor.Top)) continue;
			
		    isPlayerTouch = true;
			
		    int activeLogId = Math.Clamp(
			    ((int)(player.Position.X - Position.X) + _logAmount * _logHalfWidth) / _logWidth + 1, 1, _logAmount);
				
		    int dip = _dip[activeLogId - 1];
		    if (dip > maxDip)
		    {
			    _activeLogId = activeLogId;
			    _maxDip = dip;
				
			    // Remember current dip value for the next player
			    maxDip = _maxDip;
		    }
			
		    player.Position += new Vector2(0f, MathF.Round(dip * MathF.Sin(Mathf.DegToRad(_angle))));
	    }

	    UpdateLogPositions();

	    UpdateAngle(isPlayerTouch, Scene.Instance.Speed);
		
	    QueueRedraw();
    }

    public override void _Draw()
    {
	    if (_logTexture == null) return;
	    
        for (var i = 0; i < _logAmount; i++)
        {
            DrawTexture(_logTexture, _logPositions[i]);
        }
    }
    
    private void UpdateLogPositions()
    {
	    float sine = MathF.Sin(Mathf.DegToRad(_angle));
	    for (var i = 0; i < _logAmount; i++)
	    {
		    float logDifference = Math.Abs(i - _activeLogId + 1);
		    float logDistance = 1f - logDifference / (i < _activeLogId ? _activeLogId : _logAmount - _activeLogId + 1);

		    float dip = MathF.Round(_maxDip * MathF.Sin(Mathf.DegToRad(90f * logDistance)) * sine);
		    _logPositions[i].Y = -_logHalfWidth + dip;
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
