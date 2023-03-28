///////////////////////////////////////////////////////////////////////
////////////////// МОЯ ВЕРСИЯ попытка ;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
      BNF_text.Text = File.ReadAllText("TextFile1.txt");
    }
    public enum ScrollBarType : uint
    {
      SbHorz = 0,
      SbVert = 1,
      SbCtl = 2,
      SbBoth = 3
    }
    public enum Message : uint
    {
      WM_VSCROLL = 0x0115
    }
    public enum ScrollBarCommands : uint
    {
      SB_THUMBPOSITION = 4
    }
    [DllImport("User32.dll")]
    public extern static int GetScrollPos(IntPtr hWnd, int nBar);
    [DllImport("User32.dll")]
    public extern static int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private void text_In_VScroll(object sender, EventArgs e)
    {
      int nPos = GetScrollPos(text_In.Handle, (int)ScrollBarType.SbVert);
      nPos <<= 16; uint wParam = (uint)ScrollBarCommands.SB_THUMBPOSITION | (uint)nPos;
      SendMessage(text_numeric.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));
    }
    private void text_In_ContentsResized(object sender, ContentsResizedEventArgs e)
    {
      int linesCount = text_In.Lines.Count();
      text_numeric.Text = string.Empty;
      if (linesCount == 0) return;
      string text = string.Empty;
      for (int i = 1; i < linesCount; i++)
      { text = text + i.ToString() + Environment.NewLine; }
      text = text + linesCount.ToString();
      text_numeric.Text = text;
      text_In_VScroll(sender, e);
    }
    private void code_TextBox_MouseDown(object sender, MouseEventArgs e)
    {
      if (clearSelection)
      {
        text_In.SelectAll();
        text_In.SelectionColor = System.Drawing.Color.Black;
        text_In.SelectionBackColor = System.Drawing.Color.White;
        text_In.DeselectAll();
        clearSelection = false;
      }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    public List<string> reservedWords = new List<string> { "Начало", "Анализ", "Синтез", "Окончание" };
    List<List<string>> wordsTable;
    Dictionary<string, string> variables;
    bool clearSelection = false;
    int currentRow = 0;
    int currentWord = 0;
    bool slag = false;
    bool oper = false;
    List<string> slags;
    bool dotCommaSeen = true;
    bool  mark = false;

    public enum VariableErrorType : int
    {
      Correct = 0,
      FirstDigit = 1,
      FirstUnknownChar = 2,
      UnknownChar = 3,
      MoreLength = 4,
      Digit = 5,
      Letter = 6,
      LessLength = 7,
      ReserverWord = 8
    }

    public enum NumericErrorType : int
    {
      Correct = 0,
      NotNumeric = 1,
      Overflow = 2,
      doubleNumer = 3,
      IntNumer = 4
    }
    private void button1_Click(object sender, EventArgs e)
    {
      int linesCount = text_In.Lines.Count();
      text_Out.Text = string.Empty;
      text_Out.ForeColor = Color.Black;
      if (linesCount == 0)
      {
        MessageBox.Show("Ошибка! На вход подан пустой текст!"); return;
      }
      else
      {
        CollectAndOrganizeData();
      }
    }

    private bool CheckFirstWord(string wordToFind)
    {
      foreach (List<string> row in wordsTable.Skip(currentRow))
      {
        foreach (string word in row.Skip(currentWord))
        {
          currentWord++;
          if (word != string.Empty)
          {
            return word.Equals(wordToFind);
          }
        }
        currentRow++;
        currentWord = 0;
      }
      return false;
    }

    void changeLine(RichTextBox RTB, int line, string text)
    {
      int s1 = RTB.GetFirstCharIndexFromLine(line);
      int s2 = line < RTB.Lines.Count() - 1 ? RTB.GetFirstCharIndexFromLine(line + 1) - 1 : RTB.Text.Length;
      RTB.Select(s1, s2 - s1); RTB.SelectedText = text;
    }

    private void OutputError(string errorText)
    {
      text_Out.Text = errorText + ", строка " + (currentRow + (currentWord > 0 ? 1 : 0));
      text_Out.ForeColor = Color.Red; currentRow = currentRow - (currentWord > 0 ? 0 : 1);
      int selectionStartIndex = text_In.GetFirstCharIndexFromLine(currentRow);
      int selectionLength = 1;
      if (currentWord == 0)
      { currentWord = wordsTable[currentRow].Count - 1; }
      else { currentWord--; }
      clearSelection = true;
      if (wordsTable[currentRow][0] == string.Empty)
      {
        changeLine(text_In, currentRow, " ");
        selectionStartIndex = text_In.Find(" ", selectionStartIndex, RichTextBoxFinds.MatchCase);
      }
      else
      {
        for (int i = 0; i <= currentWord; i++)
        {
          selectionStartIndex = text_In.Find(wordsTable[currentRow][i], selectionStartIndex + (i == 0 ? 0 : 1), RichTextBoxFinds.MatchCase);
        }
        selectionLength = wordsTable[currentRow][currentWord].Length;
      }
      text_In.Select(selectionStartIndex, selectionLength);
      text_In.SelectionColor = System.Drawing.Color.Black;
      text_In.SelectionBackColor = System.Drawing.Color.Red;
    }
   

    private void PrintData()
    {
      text_Out.Text = "Полученные результаты вычислений";
      foreach (var key in variables.Keys)
      {
        text_Out.Text += Environment.NewLine + key + " = " + variables[key];
      }
      text_Out.ForeColor = Color.Black;
    }

    private void CollectAndOrganizeData()
    {
      if (wordsTable != null) wordsTable.Clear();
      wordsTable = new List<List<string>>();
      slags = new List<string>();
      clearSelection = false;
      currentRow = 0;
      currentWord = 0;
      slag = oper = false;
      if (variables != null) variables.Clear();
      variables = new Dictionary<string, string>();
      foreach (var line in text_In.Lines)
      {
        string code = line.Replace(",", " , ").Replace(";", " ; ").Replace(":", " : ").Replace("=", " = ")
          .Replace("\r\n", " ").Replace("*", " * ").Replace("/", " / ").Replace("-", " - ").Replace("+", " + ")
          .Replace("(", " ( ").Replace(")", " ) ").Replace("!", " ! ").Replace("&", " & ").Replace("|", " | ");
        RegexOptions options = RegexOptions.None;
        Regex regex = new Regex("[ ]{2,}", options);
        code = regex.Replace(code, " ");
        code = code.Trim();
        wordsTable.Add(code.Split(' ').ToList());
      }
      AnalizeLanguage();
    }

    private void AnalizeLanguage()
    {
      if (!CheckFirstWord("Начало"))
      {
        OutputError("Ошибка, программа должна начинаться со слова \"Начало\"");
        return;
      }
      else
      {
        currentRow = currentRow + 1;
        currentWord = 0;
        while (wordsTable[currentRow][currentWord] == string.Empty)
        {
          currentRow++;
        }
        if (!AnalizeSlagaemoeOperator())
        {
          return;
        }
      }
      if (CheckFirstWord("Окончание"))
      {
        foreach (List<string> row in wordsTable.Skip(currentRow))
        {
          foreach (string word in row.Skip(currentWord))
          {
            currentWord++;
            if (word != string.Empty)
            {
              OutputError("Ошибка, после слова \"Окончание\" есть текст");
              return;
            }
          }
          currentRow++;
          currentWord = 0;
        }
        PrintData();
        return;
      }
      else
      {
        OutputError("Ошибка, программа должна завершаться словом \"Окончание\"");
        return;
      }
    }

    private bool AnalizeSlagaemoeOperator()
    {
      foreach (List<string> row in wordsTable.Skip(currentRow))
      {
        while (wordsTable[currentRow][currentWord] == string.Empty)
        {
          currentRow++;
        }

        if (!slag)
        {
          while (wordsTable[currentRow][currentWord] == string.Empty)
          {
            currentRow++;
          }
          if (!((wordsTable[currentRow][currentWord] == "Анализ") || (wordsTable[currentRow][currentWord] == "Синтез")))
          {
            OutputError("Ошибка, пропущен \"Слагаемое\" или набор \"Слагаемых\"");
            return false;
          }
          else
          {
            if (!AnalizeAnnounces())
            {
              return false;
            }
            else
            {
              slag = true;
            }
          }
        }

        if (!oper)
        {
          while (wordsTable[currentRow][currentWord] == string.Empty)
          {
            currentRow++;
          }

          if (IsVariable(wordsTable[currentRow][currentWord]) != VariableErrorType.Correct && IsInt(wordsTable[currentRow][currentWord]) != NumericErrorType.Correct)
          {
            currentWord++;
            OutputError("Ошибка, \"Опер\" должен начинаться с переменной");
            return false;
          }
          else
          {
            if (!AnalizeOperators()) { return false; }
            else { oper = true; }
          }
        }

        if (!oper && slag)
        {
          OutputError("Ошибка, пропущено \"Опер\" или набор \"Опер\"");
          return false;
        }
        if (!slag && oper)
        {
          OutputError("Ошибка, пропущено \"Слагаемое\" или набор \"Слагаемых\"");
          return false;
        }

        return true;
      }
      return true;
    }

    private bool AnalizeAnnounces()
    {
      dotCommaSeen = true;
      mark = false;
      while (wordsTable[currentRow][currentWord] == "Анализ" || wordsTable[currentRow][currentWord] == "Синтез" || wordsTable[currentRow][currentWord] == string.Empty)
      {
        while (wordsTable[currentRow][currentWord] == string.Empty)
        {
          currentRow++;
        }
        if (!(wordsTable[currentRow][currentWord] == "Анализ" || wordsTable[currentRow][currentWord] == "Синтез"))
        {
          break;
        }
        if(!dotCommaSeen)
        {
          OutputError("Ошибка, пропущена \";\" между Слагаемыми");
          return false;
        }
        if (!AnalizeAnnounce())
        {
          return false;
        }
        slag = true;
      }
      if (dotCommaSeen)
      {
        OutputError("Ошибка, после ; ожидалось слагаемое");
        return false;
      }
      if (currentWord > 0) { currentWord--; }
      return true;
    }
    private bool AnalizeAnnounce()
    {
      bool dobav = false;
      bool onePeremAnaliz = false;
      //Анализ
      if (CheckFirstWord("Анализ"))
        {
          foreach (List<string> row in wordsTable.Skip(currentRow))
          {
            foreach (string word in row.Skip(currentWord))
            {
              currentWord++;
              if (word != string.Empty)
              {
                VariableErrorType varError = IsVariable(word);

                if (varError == VariableErrorType.Correct && !onePeremAnaliz)
                {
                  if (dotCommaSeen)
                  {
                    dobav = true;
                    onePeremAnaliz = true;
                    dotCommaSeen = false;
                    mark = true;
                  }
                }
                else if (word == ",")
                {
                  OutputError("Ошибка, после \"Анализ\" может идти только одна переменная");
                  return false;
                }
                else if (word == ";")
                {
                  if (!mark)
                  {
                    OutputError("Ошибка, после \"Анализ\" ожидалась переменная");
                    return false;
                  }
                  if(dotCommaSeen)
                  {
                    OutputError("Ошибка, две \";\" подряд");
                    return false;
                  }
                  else
                  {
                    dotCommaSeen = true;
                  }
                }
                else if (varError == VariableErrorType.FirstDigit)
                {
                  OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                  return false;
                }
                else if (varError == VariableErrorType.FirstUnknownChar)
                {
                  OutputError("Ошибка, недопустимый символ при перечеслении переменных");
                  return false;
                }
                else if (varError == VariableErrorType.UnknownChar)
                {
                  OutputError("Ошибка, недопустимый символ в имени переменной");
                  return false;
                }
                else if (varError == VariableErrorType.Digit)
                {
                  OutputError("Ошибка, дожна быть буква, встречена цифра");
                  return false;
                }
                else if (varError == VariableErrorType.Letter)
                {
                  OutputError("Ошибка, дожна быть цифра, встречена буква");
                  return false;
                }
                else if (varError == VariableErrorType.MoreLength)
                {
                  OutputError("Ошибка, переменная может быть максимально длинной 4 символа");
                  return false;
                }
                else if (varError == VariableErrorType.LessLength)
                {
                  OutputError("Ошибка, переменная может быть минимальной длины 2 символа");
                  return false;
                }
                else
                {
                  OutputError("Ошибка, После \"Анализ\" может идти только одна переменная");
                  return false;
                }
              }
              else
              {
                OutputError("Ошибка, после \"Анализ\" ожидалась перем или набор перем");
                return false;
              }
            }
            if (!dobav)
            {
              OutputError("Ошибка, после \"Анализ\" ожидалась перем или набор перем");
              return false;
            }
            currentRow++;
            currentWord = 0;
            slag = true;
          return true;
          }
          return true;
        }
      //"Синтез"
      else if (wordsTable[currentRow][currentWord - 1] == "Синтез")
      {
        bool commaSeen = true;
        bool markSeen = false;
        bool doubleDot = false;
        foreach (List<string> row in wordsTable.Skip(currentRow))
        {
          foreach (string word in row.Skip(currentWord))
          {
            currentWord++;
            if (word != string.Empty)
            {
              VariableErrorType varError = IsVariable(word);
              if (varError == VariableErrorType.Correct)
              {
                if (dotCommaSeen && !doubleDot)
                {
                  dotCommaSeen = false;
                  mark = true;
                  dobav = true;
                }
                if (commaSeen)
                {
                  markSeen = true;
                  dobav = true;
                  commaSeen = false;
                }
                else
                {
                  if (word == ";")
                  {
                    if (!mark)
                    {
                      OutputError("Ошибка, после \"Синтез\" ожидалась переменная");
                      return false;
                    } 
                    if (dotCommaSeen && doubleDot)
                    {
                      OutputError("Ошибка, две \";\" подряд");
                      return false;
                    }
                    else
                    {
                      dotCommaSeen = true;
                      doubleDot = true;
                    }
                  }
                  else
                    goto Found;
                }
              }
              else if (word == ",")
              {
                if (!markSeen)
                {
                  OutputError("Ошибка, после \"Синтез\" ожидалась переменная");
                  return false;
                }
                if (commaSeen)
                {
                  OutputError("Ошибка, две запятые подряд");
                  return false;
                }
                else
                {
                  commaSeen = true;
                }
              }
              else if (word == ";")
              {
                if (!mark)
                {
                  OutputError("Ошибка, после \"Синтез\" ожидалась переменная");
                  return false;
                }
                if (dotCommaSeen)
                {
                  OutputError("Ошибка, две \";\" подряд");
                  return false;
                }
                else
                {
                  dotCommaSeen = true;
                }
              }
              else if (word == "=")
              {
                OutputError("Ошибка, недопустимый символ при перечеслении переменных");
                return false;
              }  
              else if (varError == VariableErrorType.FirstDigit)
              {
                OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                return false;
              }
              else if (varError == VariableErrorType.FirstUnknownChar)
              {
                OutputError("Ошибка, недопустимый символ при перечеслении переменных");
                return false;
              }
              else if (varError == VariableErrorType.UnknownChar)
              {
                OutputError("Ошибка, недопустимый символ в имени переменной");
                return false;
              }
              else if (varError == VariableErrorType.Digit)
              {
                OutputError("Ошибка, дожна быть буква, встречена цифра");
                return false;
              }
              else if (varError == VariableErrorType.Letter)
              {
                OutputError("Ошибка, дожна быть цифра, встречена буква");
                return false;
              }
              else if (varError == VariableErrorType.MoreLength)
              {
                OutputError("Ошибка, переменная может быть максимально длинной 4 символа");
                return false;
              }
              else if (varError == VariableErrorType.LessLength)
              {
                OutputError("Ошибка, переменная может быть минимальной длины 2 символа");
                return false;
              }
            }
            else
            {
              OutputError("Ошибка, после \"Синтез\" ожидалась перем или набор перем");
              return false;
            }
          }
          if (!dobav)
          {
            OutputError("Ошибка, после \"Синтез\" ожидалась перем или набор перем");
            return false;
          }
          currentRow++;
          currentWord = 0;
          slag = true;
          if (commaSeen)
          {
            OutputError("Ошибка, ожидалась переменная после запятой");
            return false;
          }
          return true;
        }
        return true;
      Found:
        return true;
      }
      else
      {
        OutputError("Ошибка, Слагаемое должно начинаться со слов \"Анализ\" или \"Синтез\"");
        return false;
      }
    }

    private bool AnalizeOperators()
    {
      bool isEndNotFinded = false;
      while (isEndNotFinded = !CheckFirstWord("Окончание"))
      {
        if (currentWord > 0) { currentWord--; }
        if (IsVariable(wordsTable[currentRow][currentWord]) != VariableErrorType.Correct)
        {
          currentWord++;
          OutputError("Ошибка, \"Опер\" должен начинаться с метки или с переменной");
          return false;
        }
        if (!AnalizeOperator()) { return false; }
        oper = true;
        if (currentRow == wordsTable.Count && (currentWord == wordsTable[currentRow - 1].Count || currentWord == 0)) { break; }
      }
      if (!isEndNotFinded && currentWord > 0) { currentWord--; }
      return true;
    }

    private bool AnalizeOperator()
    {
      foreach (List<string> row in wordsTable.Skip(currentRow))
      {
        foreach (string word in row.Skip(currentWord))
        {
          currentWord++;
          if (word != string.Empty)
          {
            VariableErrorType errorType = IsVariable(word);
            if (errorType == VariableErrorType.Correct)
            {
              if (!CheckFirstWord("="))
              {
                OutputError("Ошибка, после переменной пропущен знак \"=\"");
                return false;
              }
              string mathResult = CheckAndExecuteMath();
              mathResult = string.Format("{0:X}", mathResult);
              if (mathResult != "err")
              {
                currentRow++; currentWord = 0;
                double value = double.Parse(mathResult);
                if (variables.ContainsKey(word))
                {
                  variables[word] = Convert.ToInt64(value).ToString();
                }
                else
                {
                  variables.Add(word, Convert.ToInt64(value).ToString());
                }
                return true;
              }
              else { return false; }
            }
            else if (errorType == VariableErrorType.Digit)
            {
              OutputError("Ошибка, дожна быть буква, встречена цифра");
              return false;
            }
            else if (errorType == VariableErrorType.Letter)
            {
              OutputError("Ошибка, дожна быть цифра, встречена буква");
              return false;
            }
            else if (errorType == VariableErrorType.MoreLength)
            {
              OutputError("Ошибка, переменная может быть максимум 4 символа");
              return false;
            }
            else if (errorType == VariableErrorType.LessLength)
            {
              OutputError("Ошибка, переменная может быть минимум 2 символа");
              return false;
            }
            else if (errorType == VariableErrorType.UnknownChar)
            {
              OutputError("Ошибка, недопустимый символ в имени переменной");
              return false;
            }
            else if (IsDouble(word) == NumericErrorType.Correct)
            {
              OutputError("Ошибка, только переменным могут быть присвоены значения, дано число");
              return false;
            }
            else if (errorType == VariableErrorType.FirstDigit)
            {
              OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
              return false;
            }
            else if (errorType == VariableErrorType.FirstUnknownChar)
            {
              OutputError("Ошибка, математическое выражение содержит недопустимый символ");
              return false;
            }
          }
        }
        currentRow++;
        currentWord = 0;
      }
      return false;
    }

    private string CheckAndExecuteMath()
    {
      List<string> words = new List<string>();//
      int openBrackets = 0; //открыть скобки
      bool prevPlus = false;//предыдущий плюс
      bool prevMin = false;//предыдущий минус
      bool prevMult = false;//предыдущее *или/
      bool prevOrAnd = false; //предыдущее и/или
      bool prevNo = false; //предыдущее !
      bool prevDig = false;//предыдущее число
      bool prevCloseBracket = false;//предыдущая закрывающая скобка
      bool prevOpenBracket = false;//предыдущая Открытая скобка
      foreach (List<string> row in wordsTable.Skip(currentRow))
      {
        foreach (string word in row.Skip(currentWord))
        {
          currentWord++;
          if (word != string.Empty)
          {
            if (word == "Анализ" || word == "Синтез")
            {
              currentWord--;
              break;
            }
            else if (word == "-")
            {
              if (prevMin || prevPlus || prevMult || prevOrAnd || prevNo)
              {
                OutputError("Ошибка, два знака математического действия подряд");
                return "err";
              }
              else
              {
                words.Add(word);
                prevMin = true;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = false;
                prevNo = false;
                prevDig = false;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
            }
            else if (word == "+")
            {
              if (!prevDig && !prevCloseBracket)
              {
                if (prevOpenBracket)
                {
                  OutputError("Ошибка, знак математического действия \"+\" после открывающейся скобки");
                  return "err";
                }
                else if (prevMin || prevPlus || prevMult || prevOrAnd || prevNo)
                {
                  OutputError("Ошибка, два знака математического действия подряд");
                  return "err";
                }
                else if (!prevDig)
                {
                  OutputError("Ошибка, знак математического действия \"+\" не может быть в начале выражения");
                  return "err";
                }
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = true;
                prevMult = false;
                prevOrAnd = false;
                prevNo = false;
                prevDig = false;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
            }
            else if (word == "*" || word == "/")
            {
              if (!prevDig && !prevCloseBracket)
              {
                if (prevOpenBracket)
                {
                  OutputError("Ошибка, знак математического действия \"" + word + "\" после открывающейся скобки");
                  return "err";
                }
                else if (prevMin || prevPlus || prevMult || prevOrAnd || prevNo)
                {
                  OutputError("Ошибка, два знака математического действия подряд");
                  return "err";
                }
                else if (!prevDig)
                {
                  OutputError("Ошибка, знак математического действия \"" + word + "\" в начале выражения правой части");
                  return "err";
                }
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = false;
                prevMult = true;
                prevOrAnd = false;
                prevNo = false;
                prevDig = false;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
            }
            else if (word == "&" || word == "|")
            {
              if (!prevDig && !prevCloseBracket)
              {
                if (prevOpenBracket)
                {
                  OutputError("Ошибка, знак математического действия \"" + word + "\" после открывающейся скобки");
                  return "err";
                }
                else if (prevMin || prevPlus || prevMult || prevOrAnd)
                {
                  OutputError("Ошибка, два знака математического действия подряд");
                  return "err";
                }
                else if (!prevDig || !prevOpenBracket || !prevCloseBracket || !prevNo)
                {
                  OutputError("Ошибка, знак математического действия \"" + word + "\" в начале выражения правой части");
                  return "err";
                }
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = true;
                prevNo = false;
                prevDig = false;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
            }
            else if (word == "!")
            {
              if (prevDig)
              {
                OutputError("Ошибка, логический оператор после числа");
                return "err";
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = false;
                prevNo = true;
                prevDig = false;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
            }

            else if (word == "(")
            {
              openBrackets++;
              if (prevCloseBracket)
              {
                OutputError("Ошибка, между скобками пропущен знак действия");
                return "err";
              }
              else if (prevDig)
              {
                OutputError("Ошибка, между скобкой и числом пропущен знак действия");
                return "err";
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = false;
                prevNo = false;
                prevDig = false;
                prevCloseBracket = false;
                prevOpenBracket = true;
              }
            }
            else if (word == ")")
            {
              openBrackets--;
              if (openBrackets < 0)
              {
                OutputError("Ошибка, лишняя закрывающая скобка");
                return "err";
              }
              if (!prevCloseBracket && !prevDig)
              {
                if (prevOpenBracket)
                {
                  OutputError("Ошибка, пустые скобки");
                }
                else
                {
                  OutputError("Ошибка, после математического действия пропущено число");
                }
                return "err";
              }
              if (prevNo)
              {
                OutputError("Ошибка, после логического действия пропущено число");
                return "err";
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = false;
                prevNo = false;
                prevDig = false;
                prevCloseBracket = true;
                prevOpenBracket = false;
              }
            }

            else if (IsDouble(word) == NumericErrorType.Correct)
            {
              if (prevDig || prevCloseBracket)
              {
                currentWord--;
                prevDig = true;
                break;
              }
              else
              {
                words.Add(word);
                prevMin = false;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = false;
                prevNo = false;
                prevDig = true;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
            }
            else if (IsVariable(word) == VariableErrorType.Correct)
            {
              if (prevDig || prevCloseBracket)
              {
                OutputError("Ошибка, два числа подряд");
                return "err";
              }
              else if (prevCloseBracket)
              {
                OutputError("Ошибка, пропущено действие между переменной и скобкой");
                return "err";
              }
              else if (variables.ContainsKey(word))
              {
                words.Add(variables[word]);
                prevMin = false;
                prevPlus = false;
                prevMult = false;
                prevOrAnd = false;
                prevNo = false;
                prevDig = true;
                prevCloseBracket = false;
                prevOpenBracket = false;
              }
              else
              {
                OutputError("Ошибка, обращение к неинициализированной переменной");
                return "err";
              }
            }
            else if (IsDouble(word) == NumericErrorType.IntNumer)
            {
              OutputError("Ошибка, могут быть заданы только вещественные числа, использовано целое число");
              return "err";
            }
            else if (IsVariable(word) == VariableErrorType.FirstDigit)
            {
              OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
              return "err";
            }
            else if (IsVariable(word) == VariableErrorType.UnknownChar)
            {
              OutputError("Ошибка, недопустимый символ в имени переменной");
              return "err";
            }
            else if (IsVariable(word) == VariableErrorType.MoreLength)
            {
              OutputError("Ошибка, переменная может быть максимально длинной 4 символа");
              return "err";
            }
            else
            {
              OutputError("Ошибка, выражение содержит недопустимый символ");
              return "err";
            }
          }
        }
        break;
      }
      if (openBrackets > 0 && prevDig)
      {
        if (prevCloseBracket)
        {
          OutputError("Ошибка, между числом и скобкой пропущен знак действия");
        }
        else { OutputError("Ошибка, два числа подряд"); }
        return "err";
      }
      else if (openBrackets > 0)
      {
        OutputError("Ошибка, не все скобки закрыты");
        return "err";
      }
      else if (prevMin || prevPlus || prevOrAnd || prevNo || prevMult)
      {
        OutputError("Ошибка, после знака действия должны идти скобка \"(\", переменная или вещественное");
        return "err";
      }
      else if (words.Count < 1)
      {
        OutputError("Ошибка, после знака \"=\" ожидалось выражение");
        return "err";
      }
      return ComputeMath(ComputeBrackets(string.Join(" ", words.ToArray())));
    }

    private string ComputeMath(string math)
    {
      RegexOptions options = RegexOptions.None;
      Regex regex = new Regex("[ ]{2,}", options);
      math = regex.Replace(math, " ").Replace(",", ".").Replace("!", " - ").Replace("&", " + ").Replace("|", " + ");
      math = Regex.Replace(math, @"\d+(\.\d+)?", m => { var x = m.ToString(); return x.Contains(".") ? x : string.Format("{0}.0", x); });
      string result = "err";
      try
      {
        result = String.Format("{0:F20}", Convert.ToDouble(new DataTable().Compute(math, "")));
      }
      catch (System.OverflowException)
      {
        OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
      }
      catch (System.DivideByZeroException)
      {
        OutputError("Возникла ошибка в процессе вычислений. Деление на ноль");
      }
      catch (System.Data.EvaluateException)
      {
        OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
      }
      return result;
    }

    private string ComputeBrackets(string math)
    {
      while (math.Contains("("))
      {
        string beforeOpen = math.Substring(0, math.IndexOf("("));
        string afterOpen = math.Substring(math.IndexOf("(") + 1);
        if (afterOpen.IndexOf("(") < afterOpen.IndexOf(")"))
        {
          afterOpen = ComputeBrackets(afterOpen);
          string inBrackets = afterOpen.Substring(0, afterOpen.IndexOf(")"));
          afterOpen = afterOpen.Substring(afterOpen.IndexOf(")") + 1);
          inBrackets = ComputeMath(inBrackets);
          math = beforeOpen + inBrackets + afterOpen;
        }
        else
        {
          string inBrackets = afterOpen.Substring(0, afterOpen.IndexOf(")"));
          afterOpen = afterOpen.Substring(afterOpen.IndexOf(")") + 1);
          inBrackets = ComputeMath(inBrackets);
          math = beforeOpen + inBrackets + afterOpen;
        }
      }
      return math;
    }

    private VariableErrorType IsVariable(string value)
    {
      if (value.Length > 4)
      {
        return VariableErrorType.MoreLength;
      }
      else if (value.Length < 2 )
      {
        return VariableErrorType.LessLength;
      }
      else
      {
        //Проверка первой буквы
        char a = value.ToUpper()[0];
        if (a >= '0' && a <= '9')
        {
          return VariableErrorType.FirstDigit;
        }
        else if (a >= 'А' && a <= 'Я'){}
        else
        {
          return VariableErrorType.FirstUnknownChar;
        }
        //Проверка цифр 
        foreach (char c in value.ToUpper().Skip(1))
        {
          if ((c >= 'А' && c <= 'Я'))
          {
            return VariableErrorType.Letter;
          }
          else if ((c >= '0' && c <= '9')){}
          else
          {
            return VariableErrorType.UnknownChar;
          }
        }
        if (reservedWords.Contains(value))
        {
          return VariableErrorType.ReserverWord;
        }
        return VariableErrorType.Correct;
      }
    }

    private NumericErrorType IsDouble(string value)
    {
      if (!value.Contains('.'))
      {
        return NumericErrorType.IntNumer;
      }
      else
      {
        string[] parts = value.Split('.');
        foreach (char a in parts[0].ToUpper())
        {
          if (!(a >= '0' && a <= '9'))
          {
            return NumericErrorType.NotNumeric;
          }
        }
        foreach (char b in parts[1].ToUpper())
        {
          if (!(b >= '0' && b <= '9'))
          {
            return NumericErrorType.NotNumeric;
          }
        }
      }
      return NumericErrorType.Correct;
    }
    private NumericErrorType IsInt(string value)
    {
      foreach (char a in value.ToUpper())
      {
        if (a == '.')
        {
          return NumericErrorType.doubleNumer;
        }
        else if (a >= '0' && a <= '9'){}
        else
        {
          return NumericErrorType.NotNumeric;
        }
      }
      return NumericErrorType.Correct;
    }
  }
}
