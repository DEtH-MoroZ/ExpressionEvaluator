using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class EvaluationManager : MonoBehaviour
{
	public Text outputUIElement;
	public string inputString;

	private float result;
	private ExpressionEvaluator e;

	public void EditInputString (string a) {
		inputString = a;
		Debug.Log("[EvaluationManager] New input string = " + inputString);

	}

	public void EvaluateInputString () {
		result = 0;
		try{
			e = new ExpressionEvaluator(inputString);
		}
		catch (Exception ex) {
			outputUIElement.text = ex.Message;


			return;
		}


		try{	result = (float)e.Evaluate(); } 
		catch (Exception ex){
			outputUIElement.text = ex.Message;

		}
		finally {
			if (e.success == true) {
				outputUIElement.text = result.ToString();}
			Debug.Log("[EvaluationManager] Success = " + e.success + "; Result = " + result.ToString());
		};

	}
}
