﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VNet.Assets;
using Boolean = System.Boolean;
using Label = System.Windows.Controls.Label;

namespace VNet
{
	/// <summary>
	/// Interaction logic for Game.xaml
	/// </summary>
	public partial class Game : Window
	{
		private Assets.Assets _assets;
		private Script _currentScript;
		private LexicalAnalysis _lexical;
		private GameEnvironment _environment;

		// Game area elements
		private TextBlock _textBlock;
		private TextBlock _nameBlock;
		private Image _backgroundImage;
		private Image _leftCharacter;
		private Image _rightCharacter;
		private Image _centerCharacter;

		private DoubleAnimation _fadeIn;
		private DoubleAnimation _fadeOut;
		private DispatcherTimer _textTimer;

		private MediaPlayer _backgroundMusicPlayer;
		private MediaPlayer _soundEffectPlayer;

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

			_backgroundImage = new Image
			{
				Name = "backgroundImage",
				Source = _environment.CurrentBackground.image,
				Stretch = Stretch.Uniform
		};
			ViewportContainer.Children.Add(_backgroundImage);
			Panel.SetZIndex(_backgroundImage, 0);

			_leftCharacter = new Image
			{
				Name = "leftCharacter",
				Opacity = 0.0,
				Stretch = Stretch.Uniform
		};
			ViewportContainer.Children.Add(_leftCharacter);
			Panel.SetZIndex(_leftCharacter, 1);

			_centerCharacter = new Image
			{
				Name = "centerCharacter",
				Opacity = 0.0,
				Stretch = Stretch.Uniform
		};
			ViewportContainer.Children.Add(_centerCharacter);
			Panel.SetZIndex(_centerCharacter, 1);

			_rightCharacter = new Image
			{
				Name = "rightCharacter",
				Opacity = 0.0,
				Stretch = Stretch.Uniform
			};
			ViewportContainer.Children.Add(_rightCharacter);
			Panel.SetZIndex(_rightCharacter, 1);

			_nameBlock = new TextBlock
			{
				Name = "nameBlock",
				FontSize = 24,
				FontWeight = FontWeights.ExtraBold,
			};
			ViewportContainer.Children.Add(_nameBlock);
			Canvas.SetLeft(_nameBlock, 50);
			Canvas.SetTop(_nameBlock, 560);
			Panel.SetZIndex(_nameBlock, 2);

			_textBlock = new TextBlock
			{
				Name = "textBlock",
				Width = 1180.0,
				Height = 120.0,
				FontSize = 21,
				FontWeight = FontWeights.Bold,
				Foreground = new SolidColorBrush(Colors.AliceBlue),
				Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
				TextWrapping = TextWrapping.Wrap
			};
			ViewportContainer.Children.Add(_textBlock);
			Canvas.SetLeft(_textBlock, 50);
			Canvas.SetTop(_textBlock, 600);
			Panel.SetZIndex(_textBlock, 2);

			_fadeIn = new DoubleAnimation
			{
				From = 0,
				To = 1,
				Duration = new Duration(TimeSpan.FromMilliseconds(500))
			};
			_fadeOut = new DoubleAnimation
			{
				From = 1,
				To = 0,
				Duration = new Duration(TimeSpan.FromMilliseconds(500))
			};

			_textTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(Settings.TextDisplaySpeedInMiliseconds)
			};
			_textTimer.Tick += TextTimerOnTick;

			_backgroundMusicPlayer = new MediaPlayer();
			_soundEffectPlayer = new MediaPlayer();

			// Show startup image


			// Process starting script and ready the gameplay for first input of user
			_currentScript = new Script(Settings.StartScriptUri);
			_lexical = new LexicalAnalysis(_currentScript);
			_currentScript.currentLine = ProcessScript();

			// Show main menu
			Settings.inGame = true;
		}

		/*
		 * Function processes script from start to finish, executing setup commands (creating characters, backgrounds, labels..)
		 */
		private int ProcessScript()
		{
			int startOfGameLine = 0;
			while(true)
			{
				string[] command = ProcessScriptLine();
				if (command == null) return startOfGameLine;
				if (Settings.SetupKeywordList.Contains(command[0]))
				{
					ExecuteCommand(command);
				}
				else if (startOfGameLine == 0)
				{
					startOfGameLine = _currentScript.currentLine - 1;
				}
			}
		}

		/*
		 * Function processes next line in current script and returns array of commands and arguments from that line
		 */
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

			string[] lineComponents = new string[7];

			while (true)
			{
				try
				{
					switch (token.Type)
					{
						// Sets the "command" for this line
						case Type.Keyword:
							if (lineComponents[0] != null)
							{
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
								else if (lineComponents[5] == null)
									lineComponents[5] = token.Lexem;
								else if (lineComponents[6] == null)
									lineComponents[6] = token.Lexem;
								break;
							}
							lineComponents[0] = token.Lexem;
							if (token.Lexem == "label")
								lineComponents[1] = (token.Location.Line + 1).ToString();
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
							else if (lineComponents[5] == null)
								lineComponents[5] = token.Lexem;
							else if (lineComponents[6] == null)
								lineComponents[6] = token.Lexem;
							break;

						// If number is in quotes adds to string in quotes, otherwise puts the number in first empty spot of command
						case Type.Number:
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
							else if (lineComponents[5] == null)
								lineComponents[5] = token.Lexem;
							else if (lineComponents[6] == null)
								lineComponents[6] = token.Lexem;
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
								else if (lineComponents[5] == null)
									lineComponents[5] = quotedString;
								else if (lineComponents[6] == null)
									lineComponents[6] = quotedString;
								quotedString = "";
							}
							break;

						// If punctuation is in quotes adds the character to string in quotes, otherwise puts it into first available spot of command
						case Type.Punctuation:
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
							else if (lineComponents[5] == null)
								lineComponents[5] = token.Lexem;
							else if (lineComponents[6] == null)
								lineComponents[6] = token.Lexem;
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
			int intValue;
			bool boolValue;
			// Runs function corresponding to the command with the variables given
			switch (command[0])
			{
				case "execute":

					break;

				case "label":
					_assets.CreateLabel(command[2], command[1]);
					break;

				case "jump":
					JumpToLabel(command[1]);
					break;

				case "character":
					if (command[3] != null)
						_assets.CreateCharacter(command[1], command[2], command[3]);
					else if (command[2] != null)
						_assets.CreateCharacter(command[1], command[2]);
					else
						_assets.CreateCharacter(command[1]);
					break;

				case "color":
					_assets.SetCharacterColor(command[1], command[2], command[3], command[4]);
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

				case "sound":
					_assets.CreateSound(command[1], command[2]);
					break;

				case "music":
					Settings.executeNext = true;
					_assets.CreateSong(command[1], command[2]);
					break;

				case "int":
					if (int.TryParse(command[2], out intValue))
					{
						_assets.CreateVariable(command[1], intValue);
					}
					break;

				case "bool":
					if (bool.TryParse(command[2], out boolValue))
					{
						_assets.CreateVariable(command[1], boolValue);
					}
					break;

				case "if":
					Variable var = _assets.variables.Find(i => i.name == command[1]);
					if (var == null) break;
					switch (var)
					{
						case Assets.Boolean boolVal:
							if (bool.TryParse(command[3], out boolValue))
							{
								if (boolValue == boolVal.value)
								{
									JumpToLabel(command[4]);
								}
							}
							break;
						case Assets.Integer intVal:
							if (int.TryParse(command[3], out intValue))
							{
								switch (command[2])
								{
									case "<" when intVal.value < intValue:
									case ">" when intVal.value > intValue:
									case "=" when intVal.value == intValue:
										JumpToLabel(command[4]);
										break;
								}
							}
							break;
					}
					break;

				case "add":
					if (int.TryParse(command[2], out intValue))
					{
						_assets.IntegerAdd(command[1], intValue);
					}
					break;

				case "subtract":
					if (int.TryParse(command[2], out intValue))
					{
						_assets.IntegerSubtract(command[1], intValue);
					}
					break;

				case "set":
					if (int.TryParse(command[2], out intValue))
					{
						_assets.IntegerSet(command[1], intValue);
					}
					else if (bool.TryParse(command[2], out boolValue))
					{
						_assets.BooleanSet(command[1], boolValue);
					}
					break;

				case "show":
					if (command[1] == "choice")
					{
						Settings.executeNext = false;
						ShowChoice(command[2]);
					}
					if (command.Contains("pause")) Settings.executeNext = false;
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

				case "clear":
					if (command.Contains("pause"))
					{
						Settings.executeNext = false;
					}
					if (command[1] != null)
					{
						ClearCharacters(command[1]);
					}
					else
					{
						ClearCharacters();
					}
					break;

				case "play":
					if (command.Contains("pause"))
					{
						Settings.executeNext = false;
					}
					PlaySound(command[1], double.TryParse(command[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var volume) ? volume : 1.0, command.Contains("r") || command.Contains("repeat"));
					break;

				case "stop":
					if (command.Contains("pause"))
					{
						Settings.executeNext = false;
					}
					switch (command[1])
					{
						case "sound":
							_soundEffectPlayer.Stop();
							break;
						case "music":
							_backgroundMusicPlayer.Stop();
							break;
						default:
							_soundEffectPlayer.Stop();
							_backgroundMusicPlayer.Stop();
							break;
					}
					break;

				case "choice":
					if (command[1] == "create")
					{
						_assets.CreateChoice(command[2]);
					}
					else
					{
						string choiceName = command[1];
						if (command[2] == "set" && command[3] == "text")
						{
							_assets.EditChoiceText(choiceName, command[4]);
						}
						else if (command[2] == "add")
						{
							_assets.AddOptionToChoice(choiceName, command[3], command[4]);
						}
					}
					break;

				default:
					Settings.executeNext = false;
					ExecuteTextCommand(command);
					break;
			}
		}

		/*
		 * Function executes a single line in the current script if it is a text line
		 */
		private void ExecuteTextCommand(string[] command)
		{
			// For protagonist
			if (command[0] == "PC")
			{
				if (command[2] == "thought")
				{
					ShowText("", command[1], true);
				}
				else
				{
					ShowText("", command[1]);
				}
				return;
			}

			// For other characters
			Character selectedCharacter = _assets.characters.Find(i => i.name == command[0]);
			ShowText(selectedCharacter != null ? selectedCharacter.name : command[0], command[1]);
		}

		/*
		 * Function changes the currently showing background to the one with the given name
		 */
		private void ShowBackground(string bgName)
		{
			
			Background selectedBackground = _assets.backgrounds.Find(i => i.name == bgName);
			if (selectedBackground != null)
			{
				_environment.CurrentBackground = selectedBackground;

				_backgroundImage.Source = selectedBackground.image;
				_backgroundImage.BeginAnimation(OpacityProperty, _fadeIn);
			}
		}

		/*
		 * Function shows a given character with the given sprite (mood) in the given position
		 */
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
					_environment.LeftCharacter = selectedCharacter;
					xOffset = 0;
					yOffset = 0;
					_leftCharacter.Source = selectedMood.image;
					_leftCharacter.BeginAnimation(OpacityProperty, _fadeIn);
					Canvas.SetLeft(_leftCharacter, xOffset);
					Canvas.SetTop(_leftCharacter, yOffset);
				}
				else if (position == "right")
				{
					_environment.RightCharacter = selectedCharacter;
					xOffset = 760;
					yOffset = 0;
					_rightCharacter.Source = selectedMood.image;
					_rightCharacter.BeginAnimation(OpacityProperty, _fadeIn);
					Canvas.SetLeft(_rightCharacter, xOffset);
					Canvas.SetTop(_rightCharacter, yOffset);
				}
				else
				{
					_environment.CenterCharacter = selectedCharacter;
					xOffset = 380;
					yOffset = 0;
					_centerCharacter.Source = selectedMood.image;
					_centerCharacter.BeginAnimation(OpacityProperty, _fadeIn);
					Canvas.SetLeft(_centerCharacter, xOffset);
					Canvas.SetTop(_centerCharacter, yOffset);
				}
			}
		}

		/*
		 * Function clears the image of character specified in argument; if no character is specified clears all characters
		 */
		private void ClearCharacters(string position = "")
		{
			if (position == "left" && _environment.LeftCharacter != null)
			{
				_leftCharacter.BeginAnimation(OpacityProperty, _fadeOut);
				_leftCharacter.Source = null;
				_environment.LeftCharacter = null;

			}
			else if (position == "center" && _environment.CenterCharacter != null)
			{
				_centerCharacter.BeginAnimation(OpacityProperty, _fadeOut);
				_centerCharacter.Source = null;
				_environment.CenterCharacter = null;
			}
			else if (position == "right" && _environment.RightCharacter != null)
			{
				_rightCharacter.BeginAnimation(OpacityProperty, _fadeOut);
				_rightCharacter.Source = null;
				_environment.RightCharacter = null;
			}
			else
			{
				if (_environment.LeftCharacter != null)
				{
					_leftCharacter.BeginAnimation(OpacityProperty, _fadeOut);
					_leftCharacter.Source = null;
					_environment.LeftCharacter = null;
				}
				if (_environment.CenterCharacter != null)
				{
					_centerCharacter.BeginAnimation(OpacityProperty, _fadeOut);
					_centerCharacter.Source = null;
					_environment.CenterCharacter = null;
				}
				if (_environment.RightCharacter != null)
				{
					_rightCharacter.BeginAnimation(OpacityProperty, _fadeOut);
					_rightCharacter.Source = null;
					_environment.RightCharacter = null;
				}
			}
		}

		/*
		 * Function saves current line text contents and starts a timer to display the text character by character
		 */
		private void ShowText(string characterName, string content, bool thought = false)
		{
			Character selChar = _assets.characters.Find(i => i.name == characterName);
			if (selChar != null)
			{
				_nameBlock.Foreground = new SolidColorBrush(selChar.color);
			}
			else
			{
				_nameBlock.Foreground = new LinearGradientBrush(
					Color.FromRgb(32, 32, 32),
					Color.FromRgb(64, 64, 64),
					45.0);
			}
			
			_nameBlock.Text = thought ? "" : characterName;
			if (thought)
				_environment.fullText = content;
			else
				_environment.fullText = "\"" + content + "\"";

			_environment.displayedText = "";
			Settings.textDisplayedFully = false;
			_textTimer.Start();
		}

		/*
		 * Function is called on each tick of the text timer, adds one character to the text
		 */
		private void TextTimerOnTick(object sender, EventArgs e)
		{
			if (_environment.displayedText.Length >= _environment.fullText.Length)
			{
				Settings.textDisplayedFully = true;
				_textTimer.Stop();
				return;
			}
			_environment.displayedText += _environment.fullText[_environment.DisplayedTextLength];
			_textBlock.Text = _environment.displayedText;
		}

		/*
		 * Shows entire line of text at once, interrupting the "typing out" animation.
		 */
		private void FinishShowingText()
		{
			_environment.displayedText = _environment.fullText;
			_textBlock.Text = _environment.displayedText;
			_textTimer.Stop();
		}

		/*
		 * Checks if requested sound is an effect or song and plays in in appropriate sound player. If sound and song have the same name the sound takes precedence.
		 */
		private void PlaySound(string soundName, double volume, bool repeat = false)
		{
			var snd = _assets.sounds.Find(i => i.name == soundName);
			if (snd == null)
			{
				snd = _assets.music.Find(i => i.name == soundName);
				if (snd == null) return;
				_backgroundMusicPlayer.Open(new Uri(snd.location));
				if (repeat)
				{
					_backgroundMusicPlayer.MediaEnded += new EventHandler(RepeatPlayback);
				}
				_backgroundMusicPlayer.Volume = volume;
				_backgroundMusicPlayer.Play();
				return;
			}
			_soundEffectPlayer.Open(new Uri(snd.location));
			if (repeat)
			{
				_soundEffectPlayer.MediaEnded += new EventHandler(RepeatPlayback);
			}
			_soundEffectPlayer.Volume = volume;
			_soundEffectPlayer.Play();
		}

		private void RepeatPlayback(object sender, EventArgs e)
		{
			var player = (MediaPlayer) sender;
			player.Position = TimeSpan.Zero;
			player.Play();
		}

		/*
		 * Pauses the gameplay and shows multiple buttons corresponding to choice options
		 */
		private void ShowChoice(string choiceName)
		{
			Settings.inChoice = true;
			int textTop = 100;
			Choice ch = _assets.choices.Find(i => i.name == choiceName);
			_textBlock.Text = ch.text;

			foreach (Option opt in ch.options)
			{
				textTop += 50;
				Button button = new Button
				{
					Name = opt.destinationLabel,
					Width = 960,
					Height = 48,
					Background = new SolidColorBrush(Color.FromArgb(192, 16, 16, 16))
				};
				TextBlock optionTextBlock = new TextBlock
				{
					Name = "optionTextBlock",
					Text = opt.text,
					FontSize = 20,
					FontWeight = FontWeights.Bold,
					Foreground = new SolidColorBrush(Colors.White)
				};
				button.Content = optionTextBlock;
				button.AddHandler(Button.ClickEvent, new RoutedEventHandler(ChoiceButtonClick));
				ViewportContainer.Children.Add(button);
				Canvas.SetLeft(button, Settings.windowWidth / 2 - button.Width / 2);
				Canvas.SetTop(button, textTop);
				Panel.SetZIndex(button, 3);
				_environment.onscreenButtonNames.Add(button.Name);
			}
		}

		/*
		 * Event handler for clicking a button in choice, clears the choice, resumes gameplay and jumps to specified label
		 */
		private void ChoiceButtonClick(object sender, RoutedEventArgs e)
		{
			JumpToLabel(((Button)sender).Name);
			Settings.inChoice = false;
			TextBlock block = (TextBlock)LogicalTreeHelper.FindLogicalNode(ViewportContainer, "choiceTextBlock");
			ViewportContainer.Children.Remove(block);
			while (_environment.onscreenButtonNames.Count > 0)
			{
				Button btn = (Button)LogicalTreeHelper.FindLogicalNode(ViewportContainer, _environment.onscreenButtonNames[0]);
				ViewportContainer.Children.Remove(btn);
				_environment.onscreenButtonNames.RemoveAt(0);
			}
			TriggerNextCommand();
		}

		/*
		 * Function sets the current line in script to location of given label
		 */
		private void JumpToLabel(string labelName)
		{
			Settings.executeNext = true;
			_currentScript.currentLine = _assets.labels.Find(i => i.name == labelName).lineNumber;
		}

		/*
		 * Executes next line of script. Can execute multiple lines if they are non-stopping
		 */
		private void TriggerNextCommand()
		{
			Settings.executeNext = true;
			while (Settings.executeNext)
			{
				string[] command = ProcessScriptLine();
				if (command == null) return;
				if (Settings.SetupKeywordList.Contains(command[0]) || command[0] == null) continue;
				
				ExecuteCommand(command);
			}
		}

		/*
		 * Click event on the window, triggers next line when ingame
		 */
		private void Game_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			// Disable if not in game
			if (!Settings.inGame)
			{
				return;
			}
			// Disable if currently in choice
			if (Settings.inChoice)
			{
				return;
			}
			// If the textBlock is still being filled
			if (Settings.textDisplayedFully == false)
			{
				FinishShowingText();
				Settings.textDisplayedFully = true;
				return;
			}
			// If text is displayed completely
			TriggerNextCommand();
		}
	}
}