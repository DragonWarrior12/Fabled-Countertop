using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

public static class Dice
{
	static Random rand = new Random();

	// matches patterns like 1d20 or 3d6 or 20d4
	static string diceExpr = @"(\d+)d(\d+)";
	static MatchEvaluator diceEval = delegate (Match match)
	{
		int num = int.Parse(match.Groups[1].Value);
		int size = int.Parse(match.Groups[2].Value);

		int result = 0;

		for (int x = 0; x < num; x++) {
			// size + 1 because maximum is exclusive and can't be returned
			result += rand.Next(1, size + 1);
		}

		//strring are easier to work with
		return $"{result}";
	};

	// matches a whole bracket, even if it contains other brackets and matches a possible number infront for implicit multiplication
	static string bracketExpr = @"(?<outside>\d*)\((?<contents>[^\(\)]*(?:\([^\(\)]*\)[^\(\)]*)*)\)";
	static MatchEvaluator bracketEval = delegate (Match match)
	{
		string outside = match.Groups["outside"].Value;
		// evaluates the expression inside the bracket then returns it, adding a multiplication when nessecary
		return (outside.Length == 0 ? "" : $"{outside}*") + intRoll(match.Groups["contents"].Value);
	};

	// matches one or more multiplication/division
	static string mulDivExpr = @"(\d+)(?:([*/])(\d+))+";
	static MatchEvaluator mulDivEval = delegate (Match match)
	{
		float result = float.Parse(match.Groups[1].Value);

		for (int x = 0; x < match.Groups[2].Captures.Count; x++) {
			if (match.Groups[2].Captures[x].Value == "*") {
				result *= float.Parse(match.Groups[3].Captures[x].Value);
			} else {
				result /= float.Parse(match.Groups[3].Captures[x].Value);
			}
		}

		return $"{result}";
	};

	// matches one or more addition/subtraction
	static string addSubExpr = @"(\d+)(?:([+-])(\d+))+";
	static MatchEvaluator addSubEval = delegate (Match match)
	{
		float result = float.Parse(match.Groups[1].Value);

		for (int x = 0; x < match.Groups[2].Captures.Count; x++) {
			if (match.Groups[2].Captures[x].Value == "+") {
				result += float.Parse(match.Groups[3].Captures[x].Value);
			} else {
				result -= float.Parse(match.Groups[3].Captures[x].Value);
			}
		}

		return $"{result}";
	};

	public static float Roll(string _dice, string _entity = null)
	{
		float result = float.NaN;

		if (Validate(_dice, _entity, out _dice)) {
			if (float.TryParse(intRoll(_dice), out float _result)) result = _result;
		}

		return result;
	}

	public static bool Validate(string _toVal, string _entity, out string valid)
	{
		valid = "";
		Stack<char> openedBrackets = new Stack<char>();
		string allowedChars = "d0123456789+-*/()";
		bool variable = false;
		string varname = "";

		for (int x = 0; x < _toVal.Length; x++) {
			if (_toVal[x] == '(') {
				// variable names can't contain brackets
				if (variable) return false;
				// keeps track of brackets that need closing
				openedBrackets.Push(')');
			}
			if (_toVal[x] == '{') {
				if (variable) return false;
				openedBrackets.Push('}');
				// curly brackets mark variable names
				variable = true;
				// don't add { to the start of variable name
				continue;
			}
			if (_toVal[x] == ')' || _toVal[x] == '}') {
				// too many closed brackets
				if (openedBrackets.Count == 0) return false;
				// wrong closing bracket
				if (openedBrackets.Peek() != _toVal[x]) return false;

				// only needed for } but doesn't change anything for )
				variable = false;

				/*if (varname != "") {
					if (_entity != null && Entity.entities[_entity].Attributes.ContainsKey(varname)) {
						string value = Entity.entities[_entity].GetAtt(varname);
						// ignore text values
						if (!value.StartsWith("\x200B")) valid += "(" + value + ")";                            // add variables
					}
					varname = "";
				}*/
				// bracket closed
				openedBrackets.Pop();
			}

			if (variable) {
				//keep track of variable name for later
				varname += _toVal[x];
			} else if (allowedChars.Contains(_toVal[x].ToString())) {
				// only keep valid characters to prevent errors evaluating
				valid += _toVal[x];
			}
		}

		// too many brackets were opened
		if (openedBrackets.Count != 0) return false;

		return true;
	}

	private static string intRoll(string _dice)
	{
		// evaluate all brackets first
		_dice = Regex.Replace(_dice, bracketExpr, bracketEval);

		// evaluate all dice
		_dice = Regex.Replace(_dice, diceExpr, diceEval);

		//evaluate maths
		_dice = Regex.Replace(_dice, mulDivExpr, mulDivEval);
		_dice = Regex.Replace(_dice, addSubExpr, addSubEval);

		return _dice;
	}
}
