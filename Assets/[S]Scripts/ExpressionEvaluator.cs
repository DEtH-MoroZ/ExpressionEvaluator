using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

//additional input to try-parse is needed to avoid cultural ./, problem
//here we use doubles to avoid possible collisions with internal unity's float

/*
https://www.youtube.com/watch?v=y_snKkv0gWc
https://www.youtube.com/watch?v=LQ-iW8jm6Mk
http://www.aboutmycode.com/net-framework/building-expression-evaluator-with-expression-trees-in-csharp-table-of-contents/
https://ru.wikipedia.org/wiki/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BE%D1%87%D0%BD%D0%BE%D0%B9_%D1%81%D1%82%D0%B0%D0%BD%D1%86%D0%B8%D0%B8
*/
//test strings
/*
1)
10 - 20 \ 30 * 40 + 50
33.33

2)
25.2 + 52.1 * (3.2-8)\32
17.385

3)
-256 * -254 
says stack empty, check it X
works with brakets

4)
-3.4 * ( -12 +16)
-13.6
*/
public class ExpressionEvaluator 
{
	private double result = 0;   

	private string inputString = "";
	private string prepairedString = "";
	public bool success = false;

	public ExpressionEvaluator (string input) {
		inputString = input;
		//check if string is empty. if empty - stop execution
		if (string.IsNullOrWhiteSpace(inputString)) {
			success = false;

			throw new Exception("Ошибка: Пустое выражение");
		}

		//check, if user missplace \ and /
		prepairedString = inputString.Replace(@"\","/");

		//check for spaces between values
		prepairedString = Regex.Replace(prepairedString, @"\s+", " "); //replace all existing whitespaces with single "space". rly didnt want to use regex.


		char a1,a2,a3;

		for (int a = 2; a < prepairedString.Length; a++) {
			a1 = prepairedString[a-2];
			a2 = prepairedString[a-1];
			a3 = prepairedString[a];

			if (Char.IsNumber(a1) && (a2 == ' ') && Char.IsNumber(a3)) {
				success = false;

				throw new Exception("Ошибка: пробелы между числами, позиция " + a);
			}

		}
		//check for letters
		for (int a = 0; a < prepairedString.Length; a++) {
			if (Char.IsLetter(prepairedString[a])) {
				success = false;

				throw new Exception("Ошибка: использование переменных запрещено, позиция переменной = " + (a+1) + "; название переменной = " + prepairedString[a]);
			}
		}
		


		prepairedString = prepairedString.Replace(" ", string.Empty);

	}

	int currentInputPosition = 0;

	public double Evaluate() {


		Stack<char> Operators = new Stack<char>();
		Stack<double> Values = new Stack<double>();

		currentInputPosition = 0;
		int operatorsCounter = 0;

		bool waitingForUnary = false;

		object token;
		object prevToken = null;

		while (currentInputPosition < prepairedString.Length) {
			token = ReadToken();


			if (
				token is char
				&&
				(char)token == '-'
				&& 
				(prevToken == null || prevToken is char) 
				)
			{

				Values.Push(0);
				waitingForUnary = true;

			}

			if (token is double ) // if its just a value
			{

				Values.Push((double)token); 
				if (waitingForUnary == true) {

					PopFunction(Values, Operators); 
					waitingForUnary = false;

				}
			}
			else if (token is char) // if operator
			{
				operatorsCounter++;
				if (prevToken is char
				    &&
				    (char)prevToken != '('
					&&
				    (char)prevToken != ')'
				   
				    &&
				    (char)token != '('
				    &&
				    (char)token != ')'
				   )
				{
					success = false;

					throw new Exception ("Ошибка: в строке ввода не может быть двух и более знаков операций, идущих подряд "); }


				if ((char)token == ')') { //empty stack till find '('

					while ((Operators.Count > 0) && ((char)(Operators.Peek()) != '(') ) {
						PopFunction(Values, Operators);

					}
					if (!Operators.Contains((char)'(')) {
						success = false;

						throw new Exception("Ошибка: несбалансированное выражение, слишком много ')'");
					}
					Operators.Pop();
				}


				else { //if new token is greater or equal priority than current operator, then calculate
					while ( 
					       	(Operators.Count != 0)  //its very important to check avability of operators first, obviously
					  		&& 
					       (GetOperatorPriority((char)token) != 0) //not '('
					       &&
							(GetOperatorPriority((char)token) <= GetOperatorPriority(Operators.Peek()) ) 
					       )
					       { 
						PopFunction(Values, Operators); 
					}
					Operators.Push((char)token); 

				}

				//for the first operator
				if (Operators.Count == 0) {
					Operators.Push((char)token);

				}
			}
			prevToken = token; //got to track down prev token for unary operation




		}


		//check balancing here
		if (Operators.Contains((char)'(')) {
			success = false;

			throw new Exception("Ошибка: несбалансированное выражение, слишком много '('");
		}

		//solution for last operator
		while (Operators.Count >= 1) {
			PopFunction(Values, Operators); 
		}






		success = true;

		result = Values.Peek();

		return result;

	}	
	// aka "tokinizer"
	// this filty constraction is nessesary to determin type of token, we got to do this to check for unary operations w/o problems
	private object ReadToken() {

		if (currentInputPosition == prepairedString.Length) { return null;} //end of input string



		if (Char.IsDigit(prepairedString[currentInputPosition])) { //get digit
			return ReadDoubleToken();
		}

		else {//get operator

			return ReadCharToken();

		}

	}

	private double ReadDoubleToken () {
		string curentDigit = "";
		while (currentInputPosition < prepairedString.Length && (char.IsDigit(prepairedString[currentInputPosition]) || prepairedString[currentInputPosition] == '.')) { 
			curentDigit += prepairedString[currentInputPosition]; 
			currentInputPosition++;
		}
		double number = Double.Parse(curentDigit, CultureInfo.InvariantCulture);
		return number;
	}
	private char ReadCharToken () {
		return prepairedString[currentInputPosition++];
	}

	private int GetOperatorPriority (char Operator) {

		//also if ')' then we calc the stuff -> stuff happens inside Evaluation function

		switch (Operator)
	
		{ 
		case '*': case '/':
			return 2;

		case '+': case '-':
			return 1;

		case '(':
			return 0;		

		default: {
			success = false;
			if (Operator == ')') { success = false; throw new Exception("Oшибка: несбалансированное выражение, слишком много ')'");}
			else {success = false; throw new Exception("Oшибка: данная операция не поддерживается '"+Operator.ToString() +"'");}
		}
				
		}

	}
    //actual calculations
    private void PopFunction(Stack<double> Values, Stack<char> Functions)
    {
        double b = Values.Pop();
        double a = Values.Pop();
        switch (Functions.Pop())
        {
            case '+':
                Values.Push(a + b);
                break;
            case '-':
                Values.Push(a - b);
                break;
            case '*':
                Values.Push(a * b);
                break;
            case '/':
                {
                    if (b == 0)
                    {
                        success = false;
                        throw new DivideByZeroException("Oшибка: данная операция не поддерживается 'деление на нуль'");
                    }
                    else
                    {
                        Values.Push(a / b);
                    }
                    break;
                }
            case '^':
                {
                    success = false;
                    throw new Exception("Oшибка: данная операция не поддерживается '^' возведение в степерь");                    
                }
        }
    }
}