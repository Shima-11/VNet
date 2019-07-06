﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VNet.Assets;

namespace VNet
{
	/// <summary>
	/// Interaction logic for Game.xaml
	/// </summary>
	public partial class Game : Window
	{
		private const string _ScriptExtension = "vnets";
		private const string _StartScriptURI = "./game." + _ScriptExtension;

		private Assets.Assets _assets;
		private Script _currentScript;
		private LexicalAnalysis _lexical;
		private GameEnvironment _environment;
		
		private TextBlock _textBlock;
		private TextBlock _nameBlock;
		private Image _backgroundImage;
		private Image _leftCharacter;
		private Image _rightCharacter;
		private Image _centerCharacter;

	public Game()
		{
			InitializeComponent();
			Startup();
		}

		private void Startup()
		{
			// Prepare the game area
			_environment = new GameEnvironment();
			_assets = new Assets.Assets();

			_backgroundImage = new Image();
			_backgroundImage.Name = "backgroundImage";
			_backgroundImage.Source = _environment.CurrentBackground.image;
			ViewportContainer.Children.Add(_backgroundImage);
			Panel.SetZIndex(_backgroundImage, 0);

			_leftCharacter = new Image
				{ Name = "leftCharacter", Opacity = 0.0 };
			ViewportContainer.Children.Add(_leftCharacter);
			Panel.SetZIndex(_leftCharacter, 1);

			_centerCharacter = new Image
				{ Name = "centerCharacter", Opacity = 0.0 };
			ViewportContainer.Children.Add(_centerCharacter);
			Panel.SetZIndex(_centerCharacter, 1);

			_rightCharacter = new Image
				{ Name = "rightCharacter", Opacity = 0.0 };
			ViewportContainer.Children.Add(_rightCharacter);
			Panel.SetZIndex(_rightCharacter, 1);

			_nameBlock = new TextBlock
				{ Name = "nameBlock", FontSize = 24, FontWeight = FontWeights.ExtraBold, Foreground = new LinearGradientBrush(Colors.AliceBlue, Colors.Aquamarine, 90.0)};
			ViewportContainer.Children.Add(_nameBlock);
			Canvas.SetLeft(_nameBlock, 240);
			Canvas.SetTop(_nameBlock, 600);
			Panel.SetZIndex(_nameBlock, 2);

			_textBlock = new TextBlock
				{ Name = "textBlock", FontSize = 27, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.WhiteSmoke)};
			ViewportContainer.Children.Add(_textBlock);
			Canvas.SetLeft(_textBlock, 240);
			Canvas.SetTop(_textBlock, 630);
			Panel.SetZIndex(_textBlock, 2);

			// Execute starting script
			_currentScript = new Script(_StartScriptURI);
			_lexical = new LexicalAnalysis(_currentScript);
			ProcessScript();
		}

		/*
		 * Function executes script from start to first displayable line (text, image...)
		 */
		private void ProcessScript()
		{
			while(true)
			{
				string[] command = ProcessScriptLine();
				if (command == null) return;
				if (Settings.SetupKeywordList.Contains(command[0]))
				{
					ExecuteCommand(command);
				}
				else
				{
					_currentScript.currentLine--;
					return;
				}
			}
		}

		private string[] ProcessScriptLine()
		{
			Token token = _lexical.GetNextToken();
			if (token.Type == Type.Eof) return null;
			if (token.Type == Type.LexError)
			{
				// TODO implement lex error recovery
			}

			bool insideQuotes = false;
			string quotedString = "";

			string[] lineComponents = new string[5];

			while (true)
			{
				try
				{
					switch (token.Type)
					{
						// Sets the "command" for this line
						case Type.Keyword:
							if (lineComponents[0] != null)
								throw new Exception(token.Location.Line.ToString());
							lineComponents[0] = token.Lexem;
							break;
						// If word is in quotes adds to string in quotes, otherwise puts the word in first empty spot of command
						case Type.Word:
							if (insideQuotes)
							{
								quotedString += token.Lexem;
							}
							else if (lineComponents[0] == null)
								lineComponents[0] = token.Lexem;
							else if (lineComponents[1] == null)
								lineComponents[1] = token.Lexem;
							else if (lineComponents[2] == null)
								lineComponents[2] = token.Lexem;
							else if (lineComponents[3] == null)
								lineComponents[3] = token.Lexem;
							else if (lineComponents[4] == null)
								lineComponents[4] = token.Lexem;
							break;

						// If number is in quotes adds to string in quotes
						case Type.Number:
							if (insideQuotes)
							{
								quotedString += token.Lexem;
							}
							break;

						// If it is an opening quote starts adding following elements to quoted string, otherwise closes quote and adds whole string to first available variable slot
						case Type.Quote:
							insideQuotes = !insideQuotes;
							if (!insideQuotes)
							{
								if (lineComponents[0] == null)
									lineComponents[0] = quotedString;
								else if (lineComponents[1] == null)
									lineComponents[1] = quotedString;
								else if (lineComponents[2] == null)
									lineComponents[2] = quotedString;
								else if (lineComponents[3] == null)
									lineComponents[3] = quotedString;
								else if (lineComponents[4] == null)
									lineComponents[4] = quotedString;
								quotedString = "";
							}
							break;

						// If punctuation is in quotes adds the character to string in quotes
						case Type.Punctuation:
							if (insideQuotes)
							{
								quotedString += token.Lexem;
							}
							break;

						case Type.Whitespace:
							if (insideQuotes)
							{
								quotedString += token.Lexem;
							}
							break;
					}
				}
				catch (Exception e)
				{
					MessageBox.Show("Error in script on line " + e.Message);
				}
				token = _lexical.GetNextToken();
				if (token.Type == Type.NewLine) break;
				if (token.Type == Type.Eof) break;
				if (token.Type == Type.LexError)
				{
					MessageBox.Show("Error in script on line " + token.Location.Line + ", column " +
					                token.Location.Column);
				}
			}

			return lineComponents;
		}

		/*
		 * Function executes a single line in the current script after it has already been processed into an array of arguments
		 */
		private void ExecuteCommand(string[] command)
		{
			// Runs function corresponding to the command with the variables given
			switch (command[0])
			{
				case "execute":
					break;
				case "label":
					break;
				case "jump":
					break;
				case "character":
					if (command[3] != null)
						_assets.CreateCharacter(command[1], command[2], command[3]);
					else if (command[2] != null)
						_assets.CreateCharacter(command[1], command[2]);
					else
						_assets.CreateCharacter(command[1]);
					break;
				case "image":
					if (command[3] == null)
					{
						_assets.CreateBackground(command[1], command[2]);
					}
					else
					{
						_assets.AddImageToCharacter(command[1], command[2], command[3]);
					}
					break;
				case "show":
					// Only parameter is name, therefore show as background
					if (command[2] == null)
					{
						ShowBackground(command[1]);
					}
					// More parameters, show as character
					else if (command[2] != null)
					{
						if (command[3] != null)
							ShowCharacter(command[1], command[2], command[3]);
						else
							ShowCharacter(command[1], command[2]);
					}
					break;
				case "play":
					break;
				case "choice":
					break;
			}
		}

		private void ExecuteTextCommand(string[] commands)
		{
			// For protagonist
			if (commands[0] == "PC")
			{
				if (commands[2] == "thought")
				{
					ShowText("", commands[1], true);
				}
				else
				{
					ShowText("", commands[1]);
				}
				return;
			}

			// For other characters
			Character selectedCharacter = _assets.characters.Find(i => i.name == commands[0]);
			ShowText(selectedCharacter != null ? selectedCharacter.name : commands[0], commands[1]);
		}

		private void ShowBackground(string bgName)
		{
			Background selectedBackground = _assets.backgrounds.Find(i => i.name == bgName);
			if (selectedBackground != null)
			{
				_environment.CurrentBackground = selectedBackground;

				_backgroundImage.Source = selectedBackground.image;
			}
		}

		private void ShowCharacter(string chName, string moodName, string position = "")
		{
			Character selectedCharacter = _assets.characters.Find(i => i.name == chName);
			Mood selectedMood = selectedCharacter?.moods.Find(i => i.name == moodName);
			if (selectedMood != null)
			{
				double xOffset;
				double yOffset;
				if (position == "left")
				{
					//_environment.LeftCharacter = selectedCharacter;
					xOffset = 0;
					yOffset = 0;
					_leftCharacter.Source = selectedMood.image;
					_leftCharacter.Opacity = 1.0;
					Canvas.SetLeft(_leftCharacter, xOffset);
					Canvas.SetTop(_leftCharacter, yOffset);
				}
				else if (position == "right")
				{
					//_environment.RightCharacter = selectedCharacter;
					xOffset = 760;
					yOffset = 0;
					_centerCharacter.Source = selectedMood.image;
					_centerCharacter.Opacity = 1.0;
					Canvas.SetLeft(_centerCharacter, xOffset);
					Canvas.SetTop(_centerCharacter, yOffset);
				}
				else
				{
					//_environment.CenterCharacter = selectedCharacter;
					xOffset = 380;
					yOffset = 0;
					_rightCharacter.Source = selectedMood.image;
					_rightCharacter.Opacity = 1.0;
					Canvas.SetLeft(_rightCharacter, xOffset);
					Canvas.SetTop(_rightCharacter, yOffset);
				}
			}
		}

		private void ShowText(string characterName, string content, bool thought = false)
		{
			_nameBlock.Text = thought ? "" : characterName;
			if (thought)
				_textBlock.Text = content;
			else
				_textBlock.Text = "\"" + content + "\"";
		}

		/*
		 * Click event on the window, triggers next line when ingame
		 */
		private void Game_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			string[] command = ProcessScriptLine();
			if (command == null) return;

			// Determines if the line is a command or text line
			if (Settings.GameKeywordList.Contains(command[0]) || Settings.SetupKeywordList.Contains(command[0]))
			{
				ExecuteCommand(command);
			}
			else
			{
				ExecuteTextCommand(command);
			}
		}
	}
}