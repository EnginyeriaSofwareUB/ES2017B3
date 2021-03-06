﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public int flags;
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
	public float health = Globals.HEALTH;
	// Canviar el color del personatge
	private SpriteRenderer spr;

	//Arm
	private GameObject arm;//Persistent
	private ArmRotation rotation;

	private GameObject goPistol;
	private Pistol pistol;

	private GameObject goPickaxe;
	private Pickaxe pickaxe;

	private GameObject goGrenadeThrower;
	private GrenadeThrower grenadeThrower;

	private int heightToDead;

	private bool keyboard;

	// Canvas HUD i text
	private Canvas HUD_player;
	private Text txtMagazine;

	[Header("Unity Stuff")]
	// Asignar la barra de vida al player
	public Image healthBar;

	private Image iconTurn;
	private bool active;

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

		goPickaxe = transform.Find ("Arm/Pickaxe").gameObject;
		pickaxe = goPickaxe.GetComponent<Pickaxe>();

		goGrenadeThrower = transform.Find ("Arm/GrenadeThrower").gameObject;
		grenadeThrower = goGrenadeThrower.GetComponent<GrenadeThrower>();

		goPickaxe.SetActive(false);
		goGrenadeThrower.SetActive(false);

		heightToDead = -12;

		if (gameObject.tag == "team1") {
			teamRed = true;
		} else {
			teamBlue = true;
		}

		keyboard = true;
		flags = 0;
	}

	// Problemes amb fisiques
	void Update () {
		// Munició per cada pollo
		if (pistol.getInfiniteAmmo () != true) {
			// Mirem la munició
			//this.GetComponentInChildren<Canvas> ().GetComponentInChildren<Text> ().text = "Bullets: " + pistol.getMagazine ().ToString () + "-" + grenadeThrower.getMagazine ().ToString();
			this.GetComponentInChildren<Canvas> ().transform.Find("txtMagazine").GetComponent<Text>().text = pistol.getMagazine ().ToString ();
			this.GetComponentInChildren<Canvas> ().transform.Find("txtMagazineGrenade").GetComponent<Text>().text = grenadeThrower.getMagazine ().ToString();
		} else {
			//this.GetComponentInChildren<Canvas> ().GetComponentInChildren<Text> ().text = "Ammo: ∞";
			this.GetComponentInChildren<Canvas> ().transform.Find("txtMagazine").GetComponent<Text>().text = "∞";
			this.GetComponentInChildren<Canvas> ().transform.Find("txtMagazineGrenade").GetComponent<Text>().text = "∞";
		}
		// HUD_player
		HUD_player = this.GetComponentInChildren<Canvas>();
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
		if (keyboard && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown("w")) && movement)
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
		if (movement && keyboard) {
			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				goPistol.SetActive(true);
				goPickaxe.SetActive(false);
				goGrenadeThrower.SetActive(false);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2)) {
				goGrenadeThrower.SetActive(true);
				goPickaxe.SetActive(false);
				goPistol.SetActive(false);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3)) {
				goPickaxe.SetActive(true);
				goPistol.SetActive(false);
				goGrenadeThrower.SetActive(false);
			}
		}
		//Pause
		if (Input.GetKeyDown ("space")) {
			rotation.setEnabledRotation (false);
			pistol.setEnabledShoot(false);
			pickaxe.setEnabledShoot (false);
			grenadeThrower.setEnabledShoot (false);
		}

		if (rb2d.position.y < heightToDead && !dead){
			killChicken();
			soundManager.PlaySound("damage");
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
			// Canviem la posició del HUD per veures bé
			HUD_player.transform.localScale = new Vector3 (0.03f, 0.03f, 0.03f);
			//Arm
			rotation.flip (h);
			deactivateArm();


		}
		// Vamos a la izq. i girem el personatge mirem la izq.
		if (h < -0.1f)
		{
			transform.localScale = new Vector3(-1f, 1f, 1f);
			// Canviem la posició del HUD per veures bé
			HUD_player.transform.localScale = new Vector3 (-0.03f, 0.03f, 0.03f);
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
			soundManager.PlaySound("jump");
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

			goPickaxe = transform.Find ("Arm/Pickaxe").gameObject;
			pickaxe = goPickaxe.GetComponent<Pickaxe>();

			goGrenadeThrower = transform.Find ("Arm/GrenadeThrower").gameObject;
			grenadeThrower = goGrenadeThrower.GetComponent<GrenadeThrower>();
        }
		this.rotation.setEnabledRotation(movement);
		this.pistol.setEnabledShoot (movement);
		this.pickaxe.setEnabledShoot (movement);
		this.grenadeThrower.setEnabledShoot (movement);
	}

	/*
 +	* Decrease health of the player
 +	*/
	public void decreaseHealth(int health){

		if (this.health > health) {
			// Mostra el dany rebut
			this.GetComponentInChildren<Canvas> ().transform.Find("txtDamage").GetComponent<Text>().text = "-"+health.ToString();
			activateInfoDamage ();
			StartCoroutine("waitSecondsInfoDamage");
			this.health -= health;
			Color color = new Color (236/255f, 137/255f, 137/255f);
			spr.color = color;
			rotation.colorDamage();
			StartCoroutine("waitSecondsHealth");
			healthBar.fillAmount = this.health / Globals.HEALTH; // Restem la barra de vida
		} else {
			// Mostra el dany rebut
			this.GetComponentInChildren<Canvas> ().transform.Find("txtDamage").GetComponent<Text>().text = "-"+health.ToString();
			activateInfoDamage ();
			StartCoroutine("waitSecondsInfoDamage");
			this.GetComponentInChildren<Canvas> ().transform.Find("txtDamage").GetComponent<Text>().text = "-"+health.ToString();
			this.health = 0;
			healthBar.fillAmount = this.health / Globals.HEALTH; // Restema la barra de vida
			killChicken();
		}
		soundManager.PlaySound("damage");
	}

	public void killChicken(){
		dead = true;
        deactivateArm();
        Destroy(this.goPistol);
        Destroy(this.goPickaxe);
		Destroy(this.goGrenadeThrower);
        Destroy(this.arm);
        GameStart.deleteChicken(this.gameObject);
        StartCoroutine("waitSecondsDead");
	}

	// Espera 2 segons abans d'eliminar el pollastre
	IEnumerator waitSecondsDead(){
		yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
	}

	// Espera 1 segons
	IEnumerator waitSecondsHealth(){
		yield return new WaitForSeconds(0.4f);
		spr.color = Color.white;
		if (rotation != null) {
			rotation.resetColor();
		}
	}

	/**
	 * Deactivate arm
	 */
	private void deactivateArm(){
		rotation.setEnabledRotation (false);
		pistol.setEnabledShoot (false);
		pickaxe.setEnabledShoot (false);
		grenadeThrower.setEnabledShoot (false);
		arm.SetActive (false);
	}
	/**
	 * Activate arm
	 */
	private void activateArm(){
		rotation.setEnabledRotation (true);
		pistol.setEnabledShoot(true);
		pickaxe.setEnabledShoot (true);
		grenadeThrower.setEnabledShoot (true);
		arm.SetActive (true);
	}

	// Activació icona jugador actual
	public void activateImage () {
		active = true;
		this.GetComponentInChildren<Canvas> ().transform.Find("iconTurn").GetComponent<Image>().enabled = active;
	}

	// Desactivació icona jugador actual
	public void desactivateImage () {
		active = false;
		this.GetComponentInChildren<Canvas> ().transform.Find("iconTurn").GetComponent<Image>().enabled = active;
	}

	public void enableKeyboard(bool keyboard){
		this.keyboard = keyboard;
	}

	// Activació info damage
	public void activateInfoDamage () {
		active = true;
		this.GetComponentInChildren<Canvas> ().transform.Find("txtDamage").GetComponent<Text>().enabled = active;
	}

	// Desactivació info damage
	public void desactivateInfoDamage () {
		active = false;
		this.GetComponentInChildren<Canvas> ().transform.Find("txtDamage").GetComponent<Text>().enabled = active;
	}

	// Espera 1 segons
	IEnumerator waitSecondsInfoDamage(){
		yield return new WaitForSeconds(0.7f);
		desactivateInfoDamage ();
	}

    //Flag collision
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "flag")
        {
            collision.gameObject.SetActive(false);
            this.flags++;
			Globals.updatePoints(gameObject.tag, 0.3f);
			FinalText.updateFlags(GameStart.currentTeam, 1);
		}
    }
}
