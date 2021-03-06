﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
	// player Prefab from inspector
	public GameObject player;
    // flag Prefab from inspector
    public GameObject flag;
	// positions for chickens in every team
	Vector3[] positionsTeam1 = { new Vector3(17f,-8f,0f), new Vector3(3f,-2f,0f),
		new Vector3(-1f,9f,0f),new Vector3(9f,-2f,0f),new Vector3(-6f,-8f,0f),
		new Vector3(-4f,-8f,0f),new Vector3(-8f,-2f,0f),new Vector3(-16f,-2f,0f),
		new Vector3(-10f,-2f,0f),new Vector3(-10f,-2f,0f)
	};
	Vector3[] positionsTeam2 = { new Vector3(6.75f,1f,0f), new Vector3(20f,-2f,0f),
		new Vector3(1f,-5f,0f),new Vector3(-15f,2f,0f),new Vector3(23f,3.33f,0f),
		new Vector3(-23f,-8f,0f),new Vector3(-17f,-8f,0f),new Vector3(21f,6f,0f),
		new Vector3(17f,4f,0f),new Vector3(1f,2f,0f)
	};

	// current chicken to move (index of chickens)
	private int teamCounter;
	public static int currentTeam, lastTeam;
	// Control cursor
	public Texture2D cursorTexture;
	public CursorMode cursorMode = CursorMode.Auto;
	public Vector2 hotSpot = Vector2.zero;
	//chicken numbers

	public static int numTeams; //hardcoded for now
	public int[] chickensPerTeam; //static number of chicken per team, for now

	public static List<LinkedList<GameObject>> squads; //array of linkedlists, one per team
	public static LinkedListNode<GameObject>[] currentChickens; // last chicken of every team who played

	LinkedListNode<GameObject> nextChicken;


	private PlayerController playerController;
	//Cameras
	private CameraFollow camFollow;

	public Camera mainCamera;
	public Camera secondCamera;

	//Camera listeners
	AudioListener mainCameraAudioLis;
	AudioListener secondCameraAudioLis;

	private Canvas cnvCurrentTeam;

	public Sprite changeBlue;
	public Sprite changeRed;

	public static GameStart instance;

	public static Image bar1, bar2;

	private int turnsToFlagSpawn;

	// Use this for initialization
	void Start()
	{
		//Camera
		mainCameraAudioLis = mainCamera.GetComponent<AudioListener>();
		secondCameraAudioLis = secondCamera.GetComponent<AudioListener>();
		
		//Points bars
		bar1 = GameObject.Find("team1_bar").GetComponent<Image>();
		bar2 = GameObject.Find("team2_bar").GetComponent<Image>();
		bar1.fillAmount = 0;
		bar2.fillAmount = 0;

		//Flag creation
		
		for(var f = 0; f < Globals.numFlags; f++)
		{
			Instantiate(flag, new Vector3(Random.Range(-1, 45), 15f, 0), Quaternion.identity);
		}


		numTeams = 2;
		teamCounter = 0;
		chickensPerTeam = new int[] { Globals.numChickens, Globals.numChickens };
		currentChickens = new LinkedListNode<GameObject>[numTeams];
		squads = new List<LinkedList<GameObject>>();
		for (var team = 0; team < numTeams; team++)
		{ // for every team
			squads.Add(new LinkedList<GameObject>());
			player.gameObject.tag = "team" + team;
			for (var i = 0; i < chickensPerTeam[team]; i++) // add the amount of chickens necessary
				squads[team].AddFirst((GameObject)Instantiate(player, (team == 0)? positionsTeam1[i]: positionsTeam2[i], Quaternion.identity));

			currentChickens[team] = squads[team].First;
		}
		playerController = squads[0].First.Value.GetComponent<PlayerController>(); // put the first chicken on play
		playerController.setMovement(true);


		ShowMainCamera ();
		camFollow = mainCamera.GetComponent<CameraFollow>();

		GameObject goCurrentTeam = GameObject.Find("TeamTurnInfo");
		cnvCurrentTeam = goCurrentTeam.GetComponent<Canvas>();

		lastTeam = currentTeam;
		// Active icon
		playerController.activateImage ();
		// Desactive icon
		StartCoroutine("waitSecondsDesactivate");

		//Cursor
		OnMouseEnter();
		turnsToFlagSpawn = Random.Range(2, 5);
	}

	// Update is called once per frame
	void Update()
	{
		/*for (var team = 0; team < numTeams; team++)
		{
			if (Globals.points[team] > Globals.MAX_POINTS)
			{
				SceneManager.LoadScene("FinalScene");
			}
		}*/
		if (Globals.changeTurn) //Code to change turn
		{
			Globals.changeTurn = false; // deactivate flag

			playerController.setMovement(false); //release the current chicken

			currentTeam = ++teamCounter % numTeams; //get the current team


			nextChicken = currentChickens[currentTeam].Next ?? squads[currentTeam].First;

			/* Assign the new chicken in play */
			playerController = nextChicken.Value.GetComponent<PlayerController>();

			
			playerController.setMovement(true);
			if (lastTeam != currentTeam){
				lastTeam = currentTeam;
                StartCoroutine(waitSecondsInformTeam(currentTeam));
            }

			camFollow.setFollower(nextChicken.Value);

			currentChickens[currentTeam] = nextChicken; //set the current chicken as the last chicken who played for this team
			// Active icon
			playerController.activateImage();
			// Desactive icon
			StartCoroutine("waitSecondsDesactivate");

            // Flag
            turnsToFlagSpawn -= 1;
			if (turnsToFlagSpawn <= 0) {
				soundManager.PlaySound("flag");
				Instantiate(flag, new Vector3(Random.Range(-1,45), 15f, 0), Quaternion.identity);
				turnsToFlagSpawn = Random.Range(2, 5);
			}	
		}
	}

	private IEnumerator waitSecondsInformTeam(int team)
    {
		ShowGeneralView();
		cnvCurrentTeam.enabled = true;

		Transform temp = cnvCurrentTeam.transform.Find("changeTeam");
		Image changeTeam = temp.GetComponent<Image>();

		if (team == 0) {
			changeTeam.sprite = changeBlue;
		}
		else {
			changeTeam.sprite = changeRed;
		}
		playerController.enableKeyboard(false);
		playerController.setMovement(false);
		Time.timeScale = 0f;

        
		float pauseEndTime = Time.realtimeSinceStartup + 2;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return 0;
        }

		Time.timeScale = 1f;
		playerController.enableKeyboard(true);
        playerController.setMovement(true);
        cnvCurrentTeam.enabled = false;
		ShowMainCamera();
	}

	public static void deleteChicken(GameObject chicken)
	{
		int team;

		for (team = 0; team < numTeams; team++) {
			if (chicken == currentChickens[team].Value)
			{
				currentChickens[team] = currentChickens[team].Previous ?? squads[team].Last;
				squads[team].Remove(chicken);
				Globals.changeTurn = true;
				break;
			}
			else if (squads[team].Remove(chicken))
				break;
		}

		Globals.updatePoints(team, 0.1f);
		FinalText.updateDeaths(team, 1);

		if (squads[team].Count == 0)
		{
			numTeams--;
			squads.RemoveAt(team);
			if (squads.Count <= 1) {
				SceneManager.LoadScene("FinalScene");
			}

			if (team < currentTeam) {
				currentTeam--;
			}
		}
	}
	// Change cursor
	void OnMouseEnter()
	{
		Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
	}

	// Funció que retarda la desactivació de l'icona de jugador actual
	IEnumerator waitSecondsDesactivate(){
		float newTime =  Globals.TIME_PER_TURN;
		yield return new WaitForSeconds(newTime-1);
		playerController.desactivateImage ();
	}
		
	//Cameras
	private void ShowGeneralView() {
		mainCamera.enabled = false;
		mainCameraAudioLis.enabled = false;

		secondCamera.enabled = true;
		secondCameraAudioLis.enabled = true;
	}

	private void ShowMainCamera() {
		mainCamera.enabled = true;
		mainCameraAudioLis.enabled = true;

		secondCamera.enabled = false;
		secondCameraAudioLis.enabled = false;
	}

	public static void setFlagPoints()
	{
		
	}

}
