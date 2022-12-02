using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharaMov : MonoBehaviour
{
	//Scriptable object which holds all the player's movement parameters. If you don't want to use it
	//just paste in all the parameters, though you will need to manuly change all references in this script

	//HOW TO: to add the scriptable object, right-click in the project window -> create -> Player Data
	//Next, drag it into the slot in playerMovement on your player

	public PlayerData Data;

	PlayerInputs inputActions;
	InputAction move;
	InputAction jump;
	InputAction attack;
	InputAction special;
	InputAction dodge;
	InputAction guard;


	#region Variables
	//Components
	public Rigidbody2D RB { get; private set; }

	//Variables control the various actions the player can perform at any time.
	//These are fields which can are public allowing for other sctipts to read them
	//but can only be privately written to.
	public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
	public bool IsWallJumping { get; private set; }
	public bool IsSliding { get; private set; }
	public bool IsAttacking { get; private set; }
	public bool BlockedMovement { get; private set; }

	//Timers (also all fields, could be private and a method returning a bool could be used)
	public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

	//Jump
	private bool _isJumpCut;
	private bool _isJumpFalling;
	private bool _asDoubleJumped;

	//Wall Jump
	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	//Attack
	private Collider2D swingHitObject;

	private Collider2D extendHitObject;

	private Vector2 _moveInput;
	public float LastPressedJumpTime { get; private set; }
	
	float LastPressedSwingTime;
	float LastPressedExtendTime;
	float LastPressedDashTime;
	float LastBrokenShieldTime;



	//Set all of these up in the inspector
	[Header("Checks")]
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
	[Space(5)]
	[SerializeField] private Transform _swingHitPoint;
	[SerializeField] private Vector2 _swingCheckSize = new Vector2(0.5f, 1f);
	[Space(5)]
	[SerializeField] private Transform _extendHitPoint;
	[SerializeField] private Vector2 _extendCheckSize = new Vector2(0.5f, 1f);
	[Space(5)]
	[SerializeField] private GameObject _shieldPoint;

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	[SerializeField] private LayerMask _ennemyLayer;
	#endregion

	private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		inputActions = new PlayerInputs();
	}

	private void OnEnable()
	{
		inputActions.Enable();
		move = inputActions.Player.Move;
		move.Enable();
		jump = inputActions.Player.Jump;
		jump.Enable();
		attack = inputActions.Player.Attack;
		attack.Enable();
		special = inputActions.Player.Special;
		special.Enable();
		dodge = inputActions.Player.Dodge;
		dodge.Enable();
		guard = inputActions.Player.Guard;
		guard.Enable();
	}

	private void OnDisable()
	{
		inputActions.Disable();
		move.Disable();
		jump.Disable();
		attack.Disable();
		special.Disable();
		dodge.Disable();
		guard.Disable();
	}

	private void Start()
	{
		SetGravityScale(Data.gravityScale);
		IsFacingRight = true;
	}

	private void Update()
	{

		#region TIMERS
		LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER
		if (CanMove())
		{
			_moveInput.x = move.ReadValue<Vector2>().x;
			_moveInput.y = move.ReadValue<Vector2>().y;

			if (_moveInput.x != 0)
				CheckDirectionToFace(_moveInput.x > 0);
		}
		else
		{
			_moveInput.x = 0;
			_moveInput.y = 0;
		}

		//Jump
		if (jump.WasPressedThisFrame() && CanMove())
		{
			OnJumpInput();
		}

		if (jump.WasReleasedThisFrame())
		{
			OnJumpUpInput();
		}

		//Dodge
		if (dodge.WasPressedThisFrame() && CanMove())
		{
			OnDodgeInput();
		}

		if (dodge.WasReleasedThisFrame())
		{
			OnDodgeUpInput();
		}

		//Swing
		if (attack.WasPressedThisFrame() && CanMove())
		{
			OnAttackInput();
		}

		if (attack.WasReleasedThisFrame())
		{
			OnAttackUpInput();
		}

		//Special
		if (special.WasPressedThisFrame() && CanMove())
		{
			OnSpecialInput();
		}

		if (special.WasReleasedThisFrame())
		{
			OnSpecialUpInput();
		}

		//Guard
		if (guard.WasPressedThisFrame() && CanMove())
		{
			OnGuardInput();
		}

		if (guard.WasReleasedThisFrame())
		{
			OnGuardUpInput();
		}

		

		#endregion

		#region COLLISION CHECKS
		if (!IsJumping)
		{
			//Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
			{
				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
				_asDoubleJumped = false;
			}

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);

		}
		
		

		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;

			if (!IsWallJumping)
				_isJumpFalling = true;

			
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
		{
			_isJumpCut = false;

			if (!IsJumping)
				_isJumpFalling = false;
		}

		//Jump
		if (CanJump() && LastPressedJumpTime > 0)
		{
			IsJumping = true;
			IsWallJumping = false;
			_isJumpCut = false;
			_isJumpFalling = false;
			Jump();
		}
		if(CanDoubleJump() && LastPressedJumpTime > 0 && !_asDoubleJumped)
        {
			DoubleJump();
			_asDoubleJumped = true;

		}

		//WALL JUMP
		else if (CanWallJump() && LastPressedJumpTime > 0)
		{
			IsWallJumping = true;
			IsJumping = false;
			_isJumpCut = false;
			_isJumpFalling = false;
			_wallJumpStartTime = Time.time;
			_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

			WallJump(_lastWallJumpDir);
		}
		#endregion

		#region SLIDE CHECKS
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;
		#endregion

		#region GRAVITY
		//Higher gravity if we've released the jump input or are falling
		if (IsSliding)
		{
			SetGravityScale(0);
		}
		else if (RB.velocity.y < 0 && _moveInput.y < 0 && _asDoubleJumped)
		{
			//Much higher gravity if holding down
			SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
			//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
		}
		else if (_isJumpCut && _asDoubleJumped)
		{
			//Higher gravity if jump button released
			SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
			RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
		}
		else if ((IsJumping || IsWallJumping || _isJumpFalling || _asDoubleJumped) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
		}
		else if (RB.velocity.y < 0)
		{
			//Higher gravity if falling
			SetGravityScale(Data.gravityScale * Data.fallGravityMult);
			//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
		}
		else
		{
			//Default gravity if standing on a platform or moving upwards
			SetGravityScale(Data.gravityScale);
		}
		#endregion
	}

	private void FixedUpdate()
	{
        //Handle Run
		if (IsWallJumping)
			Run(Data.wallJumpRunLerp);
		else
			Run(1);
		
		

		//Handle Slide
		if (IsSliding)
			Slide();

		if (Physics2D.OverlapBox(_swingHitPoint.position, _swingCheckSize, 0, _ennemyLayer))
		{
			Collider2D collider = Physics2D.OverlapBox(_swingHitPoint.position, _swingCheckSize, 0, _ennemyLayer);
			swingHitObject = collider;
		}
		else
		{
			swingHitObject = null;
		}

		if (Physics2D.OverlapBox(_extendHitPoint.position, _extendCheckSize, 0, _ennemyLayer))
		{
			Collider2D collider = Physics2D.OverlapBox(_extendHitPoint.position, _extendCheckSize, 0, _ennemyLayer);
			extendHitObject = collider;
		}
		else
		{
			extendHitObject = null;
		}

	}

	#region INPUT CALLBACKS
	//Methods which whandle input detected in Update()
	public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut() || !_asDoubleJumped)
			_isJumpCut = true;
	}

	public void OnDodgeInput()
	{
		
        if (CanDodge())
        {
			Dodge();
        }
	}

	public void OnDodgeUpInput()
	{
		
	}

	public void OnAttackInput()
    {
		if(CanSwing())
		{
			Swing();
		}
	}
	public void OnAttackUpInput()
	{
        
	}

	public void OnSpecialInput()
	{
		if (CanExtend())
		{
			Extend();
		}
	}
	public void OnSpecialUpInput()
	{

	}

	public void OnGuardInput()
	{
		if (CanGuard())
		{
			Guard();
		}
	}
	public void OnGuardUpInput()
	{
		GuardUp();
	}
	#endregion

	#region GENERAL METHODS
	public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}
	#endregion

	//MOVEMENT METHODS
	#region RUN METHODS
	private void Run(float lerpAmount)
	{
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * Data.runMaxSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0;
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - RB.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
	}

	private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}
	#endregion

	#region JUMP METHODS
	private void Jump()
	{
		//Ensures we can't call Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;



		#region Perform Jump
		//We increase the force applied if we are falling
		//This means we'll always feel like we jump the same amount 
		//(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		Data.gravityStrength = -(2 * Data.jumpHeight) / (Data.jumpTimeToApex * Data.jumpTimeToApex);
		Data.jumpForce = Mathf.Abs(Data.gravityStrength) * Data.jumpTimeToApex;

		float force = Data.jumpForce;

		if (RB.velocity.y < 0)
			force -= RB.velocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void DoubleJump()
    {
		//Ensures we can't call Jump multiple times from one press
		LastPressedJumpTime = 0;
		#region Perform Jump
		//We increase the force applied if we are falling
		//This means we'll always feel like we jump the same amount 
		//(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		/*Data.gravityStrength = -(2 * Data.doubleJumpHeight) / (Data.jumpTimeToApex * Data.jumpTimeToApex);
		Data.jumpForce = Mathf.Abs(Data.gravityStrength) * Data.jumpTimeToApex;*/

		float force = Data.jumpForce;

		if (RB.velocity.y < 0)
			force -= RB.velocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);

		/*Data.gravityStrength = -(2 * Data.jumpHeight) / (Data.jumpTimeToApex * Data.jumpTimeToApex);
		Data.jumpForce = Mathf.Abs(Data.gravityStrength) * Data.jumpTimeToApex;*/
		#endregion
	}

	private void WallJump(int dir)
	{
		//Ensures we can't call Wall Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= RB.velocity.y;

		//Unlike in the run we want to use the Impulse mode.
		//The default mode will apply are force instantly ignoring masss
		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
	{
		//Works the same as the Run but only in the y-axis
		//THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif = Data.slideSpeed - RB.velocity.y;
		float movement = speedDif * Data.slideAccel;
		//So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
		//The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
	}

	private void Dodge()
    {
		RB.AddForce(new Vector2(RB.velocity.x * Data.dashSpeed, 0));
		LastPressedDashTime = Time.time;
	}
    #endregion

    #region ATTACK METHODS

	private void Swing()
    {
		//Swing
		IsAttacking= true;
		//ANIM ICI
		if (swingHitObject != null)
		{
			//Degat sur l'objet
			print(swingHitObject.name + " s'est fait touch� par un swing");
		}

		IsAttacking = false;
		LastPressedSwingTime = Time.time;
	}

	private void Extend()
    {
		IsAttacking = true;
		BlockedMovement = true;
		//RB.velocity = new Vector2(RB.velocity.x - RB.velocity.x, RB.velocity.y);

		//Anims

		StartCoroutine(ExtendCoroutine());
		
	}

	private void Guard()
    {
		IsAttacking = true;
		BlockedMovement = true;
		int guardHealth = Data.guardHealth;
		//Anim

		_shieldPoint.GetComponent<Collider2D>().enabled = true;
		print(guardHealth);
		if(guardHealth <= 0)
        {
			GuardUp();
        }
	}

	private void GuardUp()
    {
		_shieldPoint.GetComponent<Collider2D>().enabled = false;
		IsAttacking = false;
		BlockedMovement = false;


	}
	#endregion

	#region CHECK METHODS
	public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

	private bool CanMove()
    {
		return !BlockedMovement;
    }

	private bool CanJump()
	{
		return LastOnGroundTime > 0 && !IsJumping;
	}

	private bool CanDoubleJump()
    {
        if (IsJumping || _isJumpFalling && !_asDoubleJumped)
        {
			return true;
        }
        else
        {
			return false;
        }
    }

	private bool CanWallJump()
	{
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
	{
		return IsJumping && RB.velocity.y > 0;
	}

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0;
	}

	public bool CanSlide()
	{
		if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0)
			return true;
		else
			return false;
	}

	public bool CanDodge()
    {
		if (!BlockedMovement && !IsAttacking && Time.time - LastPressedDashTime > Data.dashCooldown)
			return true;
		else 
			return false;
    }

	public bool CanSwing()
    {
		if (Time.time - LastPressedSwingTime > Data.swingCooldown && !IsAttacking)
		{
			return true;
		}
		else return false;
    }

	public bool CanExtend()
	{
		if (Time.time - LastPressedExtendTime > Data.extendCooldown && !IsAttacking && !IsJumping && !_isJumpFalling)
		{
			return true;
		}
		else return false;
	}

	public bool CanGuard()
	{
		if (Time.time - LastBrokenShieldTime > Data.guardCooldown && !IsAttacking && !IsJumping && !_isJumpFalling)
		{
			return true;
		}
		else return false;
	}
	#endregion

	#region COROUTINES
	IEnumerator ExtendCoroutine()
	{
		RB.AddForce(new Vector2(Data.extendDashSpeed * Mathf.Sign(transform.localScale.x), 0));

		if (extendHitObject != null)
		{

			//Degat sur l'objet
		}

		yield return new WaitForSeconds(Data.timeBetweenExtendedHits - 0.2f);
		
		if (extendHitObject != null)
		{
			//Degat sur l'objet
			print(extendHitObject.name + " s'est fait touch� par un extend P2");
		}

		yield return new WaitForSeconds(0.2f);
		IsAttacking = false;
		BlockedMovement = false;
		
		yield return null;
	}
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(_swingHitPoint.position, _swingCheckSize);
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(_extendHitPoint.position, _extendCheckSize);

		Gizmos.color = Color.black;
		Gizmos.DrawWireCube(_shieldPoint.transform.position, _shieldPoint.transform.lossyScale);
	}
	#endregion
}
