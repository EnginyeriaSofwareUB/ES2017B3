﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	// La força en el personatge horitzontalment perque es mogui
	public float speed = 2f;
	// La maxim velocitat del personatge
	public float maxSpeed = 5f;
	// Bool per saber si toquem terra
	public bool grounded;
	// Bool per saber si és mort
	public bool dead;
	// Bool per saber equip
	public bool teamRed;
	// Bool per saber equip
	public bool teamBlue;
	// La força de salt
	public float jumpPower = 6.5f;

	// Rigibody 2D del personatge
	private Rigidbody2D rb2d;
	// Animator del personatge per utilitzar les relacions creades a l'animador
	private Animator anim;
	// Bool salta
	private bool jump;
	// Doble salto
	private bool doubleJump;
	// Control moviment
	private bool movement = false;
	// Health
	public int health = Globals.HEALTHVALUE;
	// Canviar el color del personatge
	private SpriteRenderer spr;

	//Arm
	private GameObject arm;//Persistent
	private ArmRotation rotation;

	private GameObject goPistol;
	private Pistol pistol;


	// Use this for initialization
	void Start () {
		// Obtenim el component (del player)
		rb2d = GetComponent<Rigidbody2D>();
		// Obtenim el component animador
		anim = GetComponent<Animator>();
		spr = GetComponent<SpriteRenderer>();
		//Arm 
		arm = transform.Find("Arm").gameObject;
		rotation = arm.GetComponent<ArmRotation>();

		goPistol = transform.Find ("Arm/Pistol").gameObject;
		pistol = goPistol.GetComponent<Pistol>();

		if (gameObject.tag == "team1") {
			teamRed = true;
		} else {
			teamBlue = true;
		}

	}

	// Problemes amb fisiques
	void Update () {
		// Assignem la velocitat del personatge. Buscant el valor positiu
		anim.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));
		// Assignem si toquem el terra
		anim.SetBool("Grounded", grounded);
		// Asigenm si està mort
		anim.SetBool("Dead", dead);
		// Asigenm si és Red
		anim.SetBool("TeamRed", teamRed);
		// Asigenm si és Red
		anim.SetBool("TeamBlue", teamBlue);

		// Comprovem si estem al terra (salt de precaució)
		if (grounded) {
			doubleJump = true;
		}

		// Per detectar la tecla per saltar
		if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown("w")) && movement)
		{
			// Si tocamos el suelo
			if (grounded) {
				jump = true;
				doubleJump = true;
			} else if (doubleJump) { // Si tenim el doble salt
				jump = true;
				doubleJump = false;
			}
		}

	}

	// Evitem problemes amb fisiques (funciona per frames)
	private void FixedUpdate()
	{
		// Rectificamos la velocidad con la fricción
		Vector3 fixedVelocity = rb2d.velocity;
		// Reducimos la velocidad
		fixedVelocity.x *= 0.75f;

		if (grounded)
		{
			rb2d.velocity = fixedVelocity;
		}

		// Detectem quan apretem l'eix horizontal -1 izq, 1 derecha (direcció)
		float h = Input.GetAxis("Horizontal");

		if (!movement) {
			h = 0;
		}
		// Apliquem força fisica al rigidbody al personatge
		rb2d.AddForce(Vector2.right * speed * h);


		// Si superem la velocitat maxima x a la dreta sino a la esquerra (control maxima velocitat)
		float limitedSpeed = Mathf.Clamp(rb2d.velocity.x, -maxSpeed, maxSpeed); ;
		rb2d.velocity = new Vector2(limitedSpeed, rb2d.velocity.y);


		// Vamos a la derecha
		if (h > 0.1f)
		{
			// Assignem nou vector
			transform.localScale = new Vector3(1f, 1f, 1f);
			//Arm 
			rotation.flip (h);
			deactivateArm();


		}
		// Vamos a la izq. i girem el personatge mirem la izq.
		if (h < -0.1f)
		{
			transform.localScale = new Vector3(-1f, 1f, 1f);
			//Arm 
			rotation.flip (h);
			deactivateArm();
		}
		if(0.1f > h && h > -0.1f && !dead){
			//Debug.Log ("Arm is active!!!");
			if (movement) {
				activateArm ();
			}
		}
			
		if (jump)
		{
			//Para que cancele la velocidad vertical (controlamos el impulso)
			rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
			rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
			jump = false;
		}
			
	}

	// Torna a sortir el personatge al mateix lloc si sortim de l'escena.
	/*
	void OnBecameInvisible()
	{
		transform.position = new Vector3(-1, 0, 0);
	}
	*/

	public void setMovement(bool mov){
		//Debug.Log (mov);
		this.movement = mov;
        //Enable rotation to player
        if (this.rotation == null || this.pistol == null) // ALERTA (más de lo mismo)
        {
            arm = transform.Find("Arm").gameObject;
            rotation = arm.GetComponent<ArmRotation>();

            goPistol = transform.Find("Arm/Pistol").gameObject;
            pistol = goPistol.GetComponent<Pistol>();
        }
		this.rotation.setEnabledRotation(movement);
		this.pistol.setEnabledShoot (movement);
	}

	/*
 +	* Decrease health of the player
 +	*/
	public void decreaseHealth(int health){
		if (this.health > health) {
			this.health -= health;
			//spr.color = Color.red;
			//StartCoroutine("waitSecondsHealth");
		} else {
			this.health = 0;
			dead = true;
			deactivateArm();
			Destroy (this.goPistol);
			Destroy (this.arm);
            GameStart.deleteChicken(this.gameObject);
			StartCoroutine("waitSecondsDead");
		}
	}

	// Espera 2 segons abans d'eliminar el pollastre
	IEnumerator waitSecondsDead(){
		yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
	}

	// Espera 1 segons 
	IEnumerator waitSecondsHealth(){
		yield return new WaitForSeconds(0.5f);
		spr.color = Color.white;
	}

	/**
	 * Deactivate arm
	 */
	private void deactivateArm(){
		rotation.setEnabledRotation (false);
		pistol.setEnabledShoot (false);
		arm.SetActive (false);
	}
	/**
	 * Activate arm
	 */
	private void activateArm(){
		rotation.setEnabledRotation (true);
		pistol.setEnabledShoot(true);
		arm.SetActive (true);
	}
		
}

