﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {

	//CONSTANTS
	public static int HEALTH = 100;//change if needed
	public static int TIME_PER_TURN = 10;
	//Pistol
	public const int PISTOL_DAMAGE = 50;
	public const int PISTOL_AMMO = 10; //If limitedAmmo is true
	public static int SHOTS_PER_TURN = 2;
	
	//Pickaxe
	public const int PICKAXE_DAMAGE = 100;
	
	//STATIC
	public static int numChickens = 2; //initial chickens to gameplay
	public static int numFlags = 2; //initial chickens to gameplay
	public static bool limitedAmmo = true;
	public static int MAX_POINTS = 7000; //points to win
	internal static readonly int MAXTEAMS = 2;

	//POINTS

	public static List<int> points = new List<int>();


	//TURN CONTROL
	public static bool changeTurn = false;
	public static bool skipTurn = false;
	public static int accPoints = 0;
	public static int remainingShots = SHOTS_PER_TURN;

}
