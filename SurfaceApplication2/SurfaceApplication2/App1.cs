using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using System.Windows;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using System.Threading;
using System.Text;

namespace SurfaceApplication2
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    
    public class App1 : Microsoft.Xna.Framework.Game
    {

        #region CardGame Initialization

        public ContentManager content;
        public static Texture2D cardFont;
        public static Texture2D textFont;

        //this is for our tile engine, BMPFont
        public static int textWidth = 16;   //how many texels across for letters
        public static int textColumns = 16; //how many letters are in a row on the texture
        public static int textHeight = 32;  //how many texels down for a letter
        public static int textRows = 8;     //how many rows of letters in the texture

        

        //ANIMATED CARD DEAL DESTINATIONS
        const int cd_discard = 0;
        const int cd_hand1 = 1;
        const int cd_hand2 = 2;

        public static int cardCellWidth = 32; //size of the frames in the card font image
        public static int cardHeight = 3; //card height in cells
        public static int cardWidth = 2; //card width in cells


        //GAME STATE INFO
        //Because there is a game loop that runs no matter what we need to control
        //whether or not certain actions can occur (or if they must wait until the next loop)
        //
        //gs_playing and gs_standing are states for when the game is on (probably I should've only
        //had a single state for both of these, and controlled if the player is standing or still needs a chance
        //to hit with a seperate variable.
        //
        //gs_animating is a special state for when a card is being animated (moved from the deck to a hand)
        //so while it is moving I'm not doing anything other than moving this card
        //
        //gs_gameover means the game is ended, and we are displaying the results and waiting for the
        //player to press enter.  So the game loop will essentially do nothing except wait for enter

        public static int gamestate;
        public static int lastGameState;
        const int gs_playing = 1;
        const int gs_standing = 2;
        const int gs_gameover = 3;
        const int gs_animating = 4;

        //These are kind of subsets to the overall game state - basically whose turn it is and whether they have
        //taken a turn
        private bool playerHadTurn = false;  //make sure dealer doesn't keep taking turns until player has a change (assuming player isn't standing)
        private bool dealerStanding = false; //whether dealer has stood this round

        //declare my game font
        public BMPFont gameFont = new BMPFont();

        bool keyhit = false;

        # endregion

        # region Transfer Initialization

        GraphicsDeviceManager graphics;
        public static SpriteBatch cardBatch;
        Texture2D light;
        int elapse = 0;
        bool flag = false;
        float delay = 100f; //Time delay for flash
        string binaryResult;
        Rectangle sourceRect;
        int binaryResultLength = 0;
        int binaryposition = 0;
        int location=0;
        char[] arr;
        //string myString = "apple, igyv ijbgihn bbibno buibnhjionho kvjbnjklnhinovyjvhukmhompbjk";
        int st = 0;

        private TouchTarget touchTarget;
        private Color backgroundColor = new Color(0, 0, 0);
        private bool applicationLoadCompleteSignalled;

        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private Matrix screenTransform = Matrix.Identity;

        # endregion

        #region CardStuff
        //A class to hold cards - pretty simple a card only holds a face value and a suite
        //it also contains its own draw event and information on how to display itself
        
        public class Card : IComparable
        {
            private int face;
            public int Face { get { return face; } }

            // IComparable implementation. (sort cards by face then suite)
            int IComparable.CompareTo(object obj)
            {

                Card temp = (Card)obj;

                if (this.face > temp.face)

                    return 1;

                if (this.face < temp.face)

                    return -1;

                else if (this.suite > temp.suite)

                    return 1;

                else if (this.suite < temp.suite)

                    return -1;

                else

                    return 0;

            }

            private int suite;
            public int Suite { get { return suite; } }

            int faceXStart = 0;
            int faceYStart = 0;
            int suiteXStart = 0;
            int suiteYStart = 0;
            int cardBackX = 0*cardCellWidth;
            int cardBackY = 4*cardCellWidth;
            
            public int flipped = 0; //1 if card turned upside down
            int cardFrontX = 2 * cardCellWidth; //the front (patterned) side of the card
            int cardFrontY = 4 * cardCellWidth;

            //We will tint the card black or red, depending on the suite (the cardfont texture is white to make
            //this possible)
            Color cardTint = Color.Black;

            //this is figuring out where in the cardfont texture we get the pic for the suite and the pic
            //for the face value
            void calcTexturePosition()
            {
                if (face == 2) { faceXStart = 0; faceYStart = 0; } //which part of the texture has the face
                else if (face == 3) { faceXStart = 1 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 4) { faceXStart = 2 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 5) { faceXStart = 3 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 6) { faceXStart = 4 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 7) { faceXStart = 5 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 8) { faceXStart = 6 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 9) { faceXStart = 7 * cardCellWidth; faceYStart = 0 * cardCellWidth; }
                else if (face == 10) { faceXStart = 0 * cardCellWidth; faceYStart = 1 * cardCellWidth; }
                else if (face == 11) { faceXStart = 1 * cardCellWidth; faceYStart = 1 * cardCellWidth; }
                else if (face == 12) { faceXStart = 2 * cardCellWidth; faceYStart = 1 * cardCellWidth; }
                else if (face == 13) { faceXStart = 3 * cardCellWidth; faceYStart = 1 * cardCellWidth; }
                else if (face == 14) { faceXStart = 4 * cardCellWidth; faceYStart = 1 * cardCellWidth; }
                else if (face == 15) { faceXStart = 5 * cardCellWidth; faceYStart = 1 * cardCellWidth; }
                if (suite == 1) { suiteXStart = 2 * cardCellWidth; suiteYStart = 2 * cardCellWidth; } //d
                else if (suite == 2) { suiteXStart = 1 * cardCellWidth; suiteYStart = 2 * cardCellWidth; } //c
                else if (suite == 3) { suiteXStart = 0 * cardCellWidth; suiteYStart = 2 * cardCellWidth; } //s
                else if (suite == 4) { suiteXStart = 3 * cardCellWidth; suiteYStart = 2 * cardCellWidth; } //h

                if (suite == 1 | suite == 4) { cardTint = Color.Red; } //tint red for diamonds and hearts
                flipped = 0;

            }

            //first constructor lets you specify a faceValue and a cardSuite
            public Card(int faceValue, int cardSuite)// 2 - 15 for face value (15 is Joker), 1=d, 2=c, 3=s, 4=h
            {

                if (faceValue >= 2 & faceValue <= 15 & cardSuite >= 1 & cardSuite <= 4)
                {
                    face = faceValue;
                    suite = cardSuite;
                    calcTexturePosition();
                }
                
            }
            
            //this constructor is a little easier to input, accepts a shortform for that card
            public Card(string faceSuiteCode) //eg Kd, 10h, JKd
            {
                if (faceSuiteCode.Length == 2 | faceSuiteCode.Length == 3)
                {
                    face = 0;
                    suite = 0;
                    string f = faceSuiteCode.Substring(0, 1);
                    string is10 = faceSuiteCode.Substring(0, 2);
                    string s = faceSuiteCode.Substring(1, 1);
                    if (is10 == "10") { face = 10; s = faceSuiteCode.Substring(2, 1); }
                    else if (is10 == "JK") { face = 15; s = faceSuiteCode.Substring(2, 1); }
                    else if (f == "A") { face = 14; }
                    else if (f == "K") { face = 13; }
                    else if (f == "Q") { face = 12; }
                    else if (f == "J") { face = 11; }
                    else
                    {
                        int fI = Convert.ToInt32(f);
                        if (fI >= 2 & fI <= 10) { face = fI; }
                    }
                    if (face != 0) //face value was set, so now we'll set the suite
                    {
                        if (s == "d") { suite = 1; }   //diamonds
                        else if (s == "c") { suite = 2; }  //clubs
                        else if (s == "s") { suite = 3; } //spades
                        else if (s == "h") { suite = 4; } //hearts
                    }
                    if (suite == 0 | face == 0) { face = 0; suite = 0; } //any problems this card is zeroed
                }
                calcTexturePosition();

            }

            //this will output the shortform of the card
            string cardString()
            {
                string cardString = "";
                if (face != 0 & suite != 0)
                {
                    string f = "";
                    string s = "";


                    if (face == 2) { f = "2"; }
                    else if (face == 3) { f = "3"; }
                    else if (face == 4) { f = "4"; }
                    else if (face == 5) { f = "5"; }
                    else if (face == 6) { f = "6"; }
                    else if (face == 7) { f = "7"; }
                    else if (face == 8) { f = "8"; }
                    else if (face == 9) { f = "9"; }
                    else if (face == 10) { f = "10"; }
                    else if (face == 11) { f = "J"; }
                    else if (face == 12) { f = "Q"; }
                    else if (face == 13) { f = "K"; }
                    else if (face == 14) { f = "A"; }
                    else if (face == 15) { f = "JK"; }

                    if (suite == 1) { s = "d"; }
                    else if (suite == 2) { s = "c"; }
                    else if (suite == 3) { s = "s"; }
                    else if (suite == 4) { s = "h"; }

                    cardString = f + s;
                }
                return cardString;
            } //return Kd, 10h, JKs etc

            //this is an overloaded draw event, allowing floats to be accepted
            public void drawCard(float x, float y)
            {
                drawCard(Convert.ToInt32(x),Convert.ToInt32(y));
            }

            //draws the card at the desired x,y position (I should probably have a vector method as well)
            //this should be called during the Draw loop
            public void drawCard(int x, int y) //Draw this card at position X,Y
            {

                if (flipped==0)
                {
                    //draw the blank card
                    Rectangle textOrg = new Rectangle(cardBackX, cardBackY, cardBackX + cardWidth * cardCellWidth, cardBackY + cardCellWidth * cardHeight);
                    cardBatch.Draw(cardFont, new Vector2(x, y), textOrg, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);

                    if (face != 15)
                    {
                        //draw the face value top & bottom
                        textOrg = new Rectangle(faceXStart, faceYStart, cardCellWidth, cardCellWidth);
                        cardBatch.Draw(cardFont, new Vector2(x  , y), textOrg, cardTint, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
                        cardBatch.Draw(cardFont, new Vector2(x + cardCellWidth, y + cardCellWidth * 2), textOrg, cardTint, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
                        //draw the suit in the middle
                        textOrg = new Rectangle(suiteXStart, suiteYStart, cardCellWidth, cardCellWidth);
                        cardBatch.Draw(cardFont, new Vector2(x + cardCellWidth / 2, y + cardCellWidth), textOrg, cardTint, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
                    }
                    else
                    {
                        textOrg = new Rectangle(faceXStart, faceYStart, cardCellWidth, cardCellWidth);
                        cardBatch.Draw(cardFont, new Vector2(x + cardCellWidth / 2, y + cardCellWidth), textOrg, cardTint, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
                    }
                }
                else
                {
                    //draw the blank card
                    Rectangle textOrg = new Rectangle(cardFrontX, cardFrontY, cardFrontX + cardWidth-1, cardFrontY + cardCellWidth * cardHeight-1);
                    cardBatch.Draw(cardFont, new Vector2(x, y), textOrg, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
                    
                }
            }
        }

        //Although I called it a Deck (bad planning), this class actually manages a Deck and 
        //two hands, plus a discard pile
        public class Deck
        {

            Random cardSeed = new Random();  //random number generator handy for shuffling etc
            List<Card> deck = new List<Card>();  //List of class Card making up the deck
            List<Card> hand1 = new List<Card>(); //First Hand's Cards
            List<Card> hand2 = new List<Card>(); //Second Hand's Cards
            List<Card> discardPile = new List<Card>(); //Discard Pile's Cards
            bool cardInTransit = false;  //basically whether we are animating a card right now
            int cardDestPile = 0;       //this is an bit kludgy IMO, but if a card is in transit this is
            //where it is going (see the constants declared in Game1 starting with
            //cd_
            Vector2 activeCardDestPos = Vector2.Zero;  //screen position that the card in transit wants to go
            Vector2 activeCardPosition = Vector2.Zero; //current screen position of the card in transit
            public Vector2 deckPosition = new Vector2(0, 0);  //screen position of the deck
            public Vector2 hand1Position = new Vector2(0, 256);  //screen position of the first Hand
            public Vector2 hand2Position = new Vector2(0, 128);
            public Vector2 discardPosition = new Vector2(16, 128);

            Card activeCard = new Card("00");  //the Card object of the card in transit

            bool useJokers = false;  //whether we will add Jokers in this deck

            //call in the Initialize loop to make sure we've cleared all the stacks of cards
            public void Initialize()
            {
                deck.Clear();
                hand1.Clear();
                hand2.Clear();
                discardPile.Clear();
            }

            //This let's us transfer a card from one stack of cards (a List of Card objects) to another
            //(dest is that kludgy card identifier)
            // number is how many cards to deal
            // the method will return false if there weren't enough cards to deal
            private bool deal(List<Card> fromStack, List<Card> toStack, int number, int dest)
            {
                bool enoughCards = true;

                for (int dealCtr = 0; dealCtr < number; dealCtr++)
                {
                    int lastCardNumber = fromStack.Count - 1;
                    if (lastCardNumber != -1)
                    {
                        Card dealCard = fromStack[lastCardNumber];
                        toStack.Add(dealCard);
                        fromStack.Remove(fromStack[lastCardNumber]);
                    }
                    else { enoughCards = false; }
                }

                return enoughCards;
            }

            //This is a special way to deal, sends one card from the deck to that destination pile
            //(remember those cd_ constants to pick which one)
            //When you call this, if there are cards left in the deck the game is put into the animating state
            //so nothing will be done except move the active card (MAKE SURE YOU CALL THIS DECK'S UPDATE METHOD!)
            public bool deal_animated(int dest)
            {
                bool enoughCards = true;

                int lastCardNumber = deck.Count - 1;

                if (lastCardNumber != -1)
                {
                    activeCard = deck[lastCardNumber];
                    deck.Remove(deck[lastCardNumber]);
                    lastGameState = gamestate;  //save the old game state so we can let the game go back to what it was doing after the card moves
                    gamestate = gs_animating;
                    cardInTransit = true; //let our Update loop we're moving a card
                    cardDestPile = dest;
                    if (dest == cd_hand1) { activeCardDestPos = hand1Position; }
                    else if (dest == cd_hand2) { activeCardDestPos = hand2Position; }
                    else if (dest == cd_discard) { activeCardDestPos = discardPosition; }
                    activeCardPosition = deckPosition;
                    activeCardPosition.X += deck.Count * cardWidth; //we want to start this card from the edge of the deck
                }
                else { enoughCards = false; }

                return enoughCards;
            }

            //public Method to deal Hand1 number amount of cards (just calls our generic deal method)
            public bool DealHand1(int number)
            {
                return deal(deck, hand1, number, cd_hand1);
            }
            public bool DealHand2(int number)
            {
                return deal(deck, hand2, number, cd_hand2);
            }

            //constructor for a Deck..specify how many packs of cards to use, and whether to include Jokers
            public Deck(int numPacks, bool UseJokers)
            {
                useJokers = UseJokers;
                Initialize();
                buildDeck(numPacks);
            }

            //overloaded constructor - create the deck with numPacks worth of cards (defaults to no jokers)
            public Deck(int numPacks)
            {
                Initialize();
                buildDeck(numPacks);
            }

            //build the deck with numPacks of cards
            public void buildDeck(int numPacks)
            {
                int deckCtr = 0;
                //count packs
                for (int packCtr = 0; packCtr <= numPacks; packCtr++)
                {
                    //count suites
                    for (int steCtr = 1; steCtr <= 4; steCtr++)
                    {
                        //count face cards
                        for (int fceCtr = 2; fceCtr <= 14; fceCtr++)
                        {
                            deck.Add(new Card(fceCtr, steCtr));
                        }
                        deckCtr++;
                    }
                    //if we are using jokers create them as well
                    if (useJokers)
                    {
                        deck.Add(new Card("JKh")); //Red Joker
                        deck.Add(new Card("JKc")); //Black Joker
                    }
                }

            }

            //call this in the Draw loop of the game, basically draws the deck, hands, and discard
            public void Draw()
            {
                drawStack(deckPosition, false, deck);
                drawStack(discardPosition, true, discardPile);
                drawLayout(hand2Position, hand2);
                drawLayout(hand1Position, hand1);
                if (cardInTransit) { activeCard.drawCard(activeCardPosition.X, activeCardPosition.Y); }
            }

            //draw a card Layout - basically draw each card in a stack of cards across the screen
            private void drawLayout(Vector2 pos, List<Card> stackToDraw)
            {
                drawLayout(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y), stackToDraw);
            }
            private void drawLayout(int x, int y, List<Card> stackToDraw)
            {

                //deck[deckCtr].drawCard(x+deckCtr * 32f, y);
                int c = 0;
                int r = 0;
                foreach (Card card in stackToDraw)
                {
                    card.drawCard(x + c * (cardCellWidth * cardWidth * 2 / 3 + 1), y + r * (cardCellWidth * cardHeight * +1));
                    c++;
                    if (c > 10) { c = 0; r++; }
                }


                //cardBatch.Draw(cardFont, new Vector2(x+deckCtr*32, y), Color.White);

            }

            //draw a stack of cards - basically a tight group of hidden cards
            //ShowTop specifies whether top card should be shown or hidden
            private void drawStack(Vector2 pos, bool ShowTop, List<Card> stackToDraw)
            {
                drawStack(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y), ShowTop, stackToDraw);
            }
            private void drawStack(int x, int y, bool showTop, List<Card> stackToShow)
            {
                int ctr = 0;
                int cardCtr = 0;
                foreach (Card card in stackToShow)
                {
                    int wasFlipped = card.flipped;
                    card.flipped = 1;
                    ctr++;
                    cardCtr++;
                    if (cardCtr == (stackToShow.Count) & showTop) { card.flipped = 0; }
                    card.drawCard(x + ctr * 2, y);
                    card.flipped = wasFlipped;
                }

            }

            //Shuffle routine will shuffle one of our lists of cards
            private void shuffle(List<Card> stackToShuffle)
            {
                //create an empty deck to hold our shuffled cards
                List<Card> tempDeck = new List<Card>();

                //can't just shuffle once!  So we will shuffle at least 10 times, up to 19 times total
                int timesToShuffle = 10 + cardSeed.Next(10);

                for (int shuffleCounter = 0; shuffleCounter < timesToShuffle; shuffleCounter++)
                {
                    foreach (Card card in stackToShuffle) //go through our stack of cards
                    {
                        int coin = cardSeed.Next(2);  //is this card going to the top or bottom of our temporary stack?
                        if (coin == 0)
                        {
                            tempDeck.Insert(0, card);
                        }
                        else
                        {
                            tempDeck.Add(card);
                        }
                    }

                    stackToShuffle.Clear();  //clear the stack we're supposed to shuffle

                    foreach (Card card in tempDeck)  //copy our temp deck to our deck
                    {
                        stackToShuffle.Add(card);
                    }

                    tempDeck.Clear(); //clear the temporary deck
                }

            }

            //shuffles the actual deck of cards (usually just after you create the deck, you like to shuffle it!)
            public void shuffleDeck() { shuffle(deck); }

            //this method will sort a stack of cards by their suite and value(so you can sort the player's hand for example)
            private void sortByFace(List<Card> stackToSort)
            {
                stackToSort.Sort();
            }

            //make sure this is called in the game's Update loop - if a card is being animated we'll update it
            public void Update()
            {
                if (cardInTransit) { moveActiveCard(); }
            }

            //actual card to move a card if we are animating, when the card gets to its destination this
            //will reset the gamestate back to its old state so other things can happen
            private void moveActiveCard()
            {
                Vector2 posDiff = activeCardDestPos - activeCardPosition;
                if (posDiff.Length() > 5f)
                {
                    posDiff.Normalize();
                    posDiff *= 6;
                    activeCardPosition += posDiff;
                }
                else
                {
                    gamestate = lastGameState;
                    cardInTransit = false;
                    if (cardDestPile == cd_hand1) { hand1.Add(activeCard); }
                    else if (cardDestPile == cd_hand2) { hand2.Add(activeCard); }
                    else if (cardDestPile == cd_discard) { discardPile.Add(activeCard); }

                }
            }

            //Special code to get the value of a stack of cards, according to BlackJack rules
            public int sumStackBJ(List<Card> stackToSum)
            {
                int value = 0;
                int aces = 0;
                foreach (Card card in stackToSum)
                {
                    int face = card.Face;
                    if (face >= 2 & face <= 10) { value += face; }
                    else if (face >= 11 & face <= 13) { value += 10; }
                    if (face == 14) { aces += 1; }
                }

                //aces are worth 11 unless this will bust the player, then they are worth 2
                for (int aCtr = 1; aCtr <= aces; aCtr++)
                {
                    if (value == 9 & aCtr < aces) { value += 2; } //in case there is more than one ace
                    else if (value <= 10) { value += 11; }
                    else { value += 2; }
                }

                return value;
            }

            public int hand1SumBJ()
            {
                return sumStackBJ(hand1);
            }
            public int hand2SumBJ()
            {
                return sumStackBJ(hand2);
            }

        }

        //the BMPFont class is a quicky tile rendering engine
        public class BMPFont
        {

            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";  //these are the characters in the set in order

            public BMPFont()
            {


            }

            //call in the Draw even, draw a string at x,y and tint it (so the actualy font texture should be white!)
            public void drawText(string msg, int x, int y, Color Tint)
            {
                int ctr = 0;
                foreach (char letter in msg)  //cycle through the letters in the message
                {
                    int pos = letters.IndexOf(letter);
                    if (pos == -1) { pos = 0; } //if we're using an invalid character just show the first character

                    //y coordinate on the texture of this letter
                    //to break up into rows we divide by the number of columns - this will be rounded because
                    //we are using an integer (textColumns is a public variable set
                    //  ...probably I should be able to calculate it based on the width and height of each letter
                    //     vs the width and height of the texture but I haven't gotten around to it!
                    int yPos = pos / textColumns;

                    //x coordinate of this texture
                    //we just find the remainder of how many rows down times the number of columns
                    int xPos = pos - yPos * textColumns;

                    //this rectangle usings our starting x and y multiplied by the height and width of the
                    //letters in the texture file
                    Rectangle src = new Rectangle(textWidth * xPos, textHeight * yPos, textWidth, textHeight);

                    //this method of Draw let's us draw only a portion of the texure
                    cardBatch.Draw(textFont, new Vector2(x + ctr * textWidth, y), src, Tint, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
                    ctr++;
                }
            }


        }

        Deck deck = new Deck(1, false);

        #endregion

        # region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            content = new ContentManager(Services);
        }
        # endregion

        # region Surface Methods

        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected TouchTarget TouchTarget
        {
            get { return touchTarget; }
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            graphics.ApplyChanges();
            // Make sure the window is in the right location.
            Program.PositionWindow();
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;
            
            /*
            touchTarget = new TouchTarget((IntPtr)0, EventThreadChoice.OnBackgroundThread); // I Want to capture all runing application events.    
            touchTarget.EnableInput();
            touchTarget.TouchDown += new EventHandler<TouchEventArgs>(touchTarget_TouchDown);


            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();
             */
        }

        # endregion

        
        #region TouchInput Recognization
        /*
        //recognises touch input
        void touchTarget_TouchDown(object sender, TouchEventArgs e)
        {
            string touchEvent = e.ToString();
            char[] delimeters = { '(', ')' };
            string[] position = touchEvent.Split(delimeters);
            string pos = position[1];
            string[] axis = pos.Split(',', ' ');
            int xAxis = int.Parse(axis[0]);
            int yAxis = int.Parse(axis[2]);
            string[] values = touchEvent.Split('=');
            //Console.WriteLine(values[4]);
            String[] BinaryArray = new String[8];
            string touchType = "blob";
            
            
            switch(values[4].Equals(touchType) && (xAxis>1250) && (xAxis<1900) && (yAxis>0) && (yAxis<500))
                    {
                        case 1: ((xAxis > 1550) && (xAxis < 1600) && (yAxis > 150) && (yAxis < 200))
                            
                            break;

                        case 2:
                            //...
                            break;

                        case 3:
                            //...
                            break;

                        case 4:
                           // ...
                            break;

                        default:
                            //...
                            break;


                    }
             
            
            if (values[4].Equals(touchType) && (xAxis > 1250) && (xAxis < 1900) && (yAxis > 0) && (yAxis < 500))
            {
                Console.WriteLine("X Axis:");
                Console.WriteLine(xAxis);
                Console.WriteLine("Y Axis:");
                Console.WriteLine(yAxis);
                
                if ((xAxis > 1550) && (xAxis < 1600) && (yAxis > 150) && (yAxis < 200))
                {
                    BinaryArray[1] = "1";
                    Console.WriteLine("Point A");
                }
                else{
                    BinaryArray[1] = "1";
                }
                if ((xAxis > 1600) && (xAxis < 1650) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point B");
                }
                else if ((xAxis > 1650) && (xAxis < 1700) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point C");
                }
                else if ((xAxis > 1700) && (xAxis < 1750) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point D");
                }
                else if ((xAxis > 1750) && (xAxis < 1800) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point E");
                }
                else if ((xAxis > 1800) && (xAxis < 1850) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point F");
                }
                else if ((xAxis > 1850) && (xAxis < 1900) && (yAxis > 150) && (yAxis < 200))
                {
                    Console.WriteLine("Point G");
                }
               


            }


        }
           */  
        # endregion
        

        # region Initialize, Load, UnloadContents
        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            //BlackJack Methods
            deck.Initialize();
            deck.buildDeck(1);
            deck.shuffleDeck();
            deck.DealHand1(1);
            deck.DealHand2(1);
            gamestate = gs_playing;
            playerHadTurn = false;
            dealerStanding = false;
           
            //End

            // TODO: Add your initialization logic here

            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Set the application's orientation based on the orientation at launch
            currentOrientation = ApplicationServices.InitialOrientation;

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            // Setup the UI to transform if the UI is rotated.
            // Create a rotation matrix to orient the screen so it is viewed correctly
            // when the user orientation is 180 degress different.
            Matrix inverted = Matrix.CreateRotationZ(MathHelper.ToRadians(180)) *
                       Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                 graphics.GraphicsDevice.Viewport.Height,
                                                 0);

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }

            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per app and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Vector2 spritePosition = Vector2.Zero;
            
            cardBatch = new SpriteBatch(graphics.GraphicsDevice);
            light = Content.Load<Texture2D>("flash2");                        //testing different flash
            //light = Content.Load<Texture2D>("flash");

            // TODO: use this.Content to load your application content here

                //this is the texture used to display a playing card
                cardFont = Content.Load<Texture2D>("cardfont");

                //this is the simple bitmap font for this game so we can display text
                //(hopefully when XNA is released we'll be able to handle text more easily, but
                //  in the meantime at least it was good practice!)
                textFont = Content.Load<Texture2D>("font");

                //this is our main sprite batch, i'm using a single batch for the whole game,
                //so the deck and card drawing methods are actually using this spritebatch
               

        }

        /// <summary>
        /// UnloadContent will be called once per app and is the place to unload
        /// all content.
        /// </summary>
        protected void UnloadContent(bool unloadAllContent)
        {
            // TODO: Unload any non ContentManager content here
            if (unloadAllContent == true)
            {
                content.Unload();
            }
        }

        # endregion

        # region update
        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //ALWAYS update our deck object, because if we're animating a card and we don't call it
            //the game will be stuck in animating mode forever (ie the game will freeze)
            deck.Update();

            if (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)
            {
                if (ApplicationServices.WindowAvailability == WindowAvailability.Interactive)
                {
                    // TODO: Process touches, 
                    // use the following code to get the state of all current touch points.
                    //ReadOnlyTouchPointCollection touches = touchTarget.GetState();  //Seth's Testing
                    //Console.WriteLine(touches.ToString());
                    //Console.WriteLine(touchTarget.ToString());
                }

                if (flag == true)
                {
                    elapse += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (elapse % delay == 0)
                    {
                        if (location < arr.Length)
                        {
                            location++;
                        }
                        else
                        {
                            location = 0;
                            //flag = false;
                        }
                        st = 1;
                    }
                    else
                        st = 0;

                    sourceRect = new Rectangle(0, 0, 100, 100);
                    //destRect = new Rectangle(frame*100, frame*100, 100, 100);
                }
            }
            if (gamestate != gs_animating) //if we're in animating mode we don't want to process anything else
            {
                if (gamestate != gs_gameover) { results(); } //before we do anything else, add up the cards and end the game if necessary

                if (gamestate != gs_gameover) { playRound(); } //if the game isn't over then play on!

                base.Update(gameTime);

                if (gamestate == gs_gameover) //if the game IS over then we'll wait for the enter key then start again
                {
                    KeyboardState newState = Keyboard.GetState();
                    if (newState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter)) { this.Initialize(); }
                }
            }

        }
        # endregion

        # region play game methods
        //code for actual gameplay
        void playRound()
        {
            //if the player is actually still playing (ie he still wants new cards and isn't standing)
            if (gamestate == gs_playing)
            {
                if (!playerHadTurn | dealerStanding) { playerturn(); }
                else if (!dealerStanding) { dealerturn(); }
            }
            //if the player is standing, but the dealer isn't done then let the dealer play
            else if (gamestate == gs_standing & !dealerStanding)
            {
                dealerturn();
            }

            //if the player is done and the dealer is done, then the game is over!
            else if (gamestate == gs_standing & dealerStanding) { gamestate = gs_gameover; }


        }

        //code to see if the game is over due to blackjack, or busting
        void results()
        {
            int playerscore = deck.hand1SumBJ();
            int dealerscore = deck.hand2SumBJ();
            if (playerscore >= 21 | dealerscore >= 21) { gamestate = gs_gameover; } //if anyone got more than 21 the game is over
            //if the player is standing and the dealer's score is better the game is over (because the dealer won't play anymore)
            if (gamestate == gs_standing & dealerscore >= playerscore) { gamestate = gs_gameover; }

        }

        //code for the player's move
        void playerturn()
        {

            KeyboardState newState = Keyboard.GetState();

            //This will reset our keypress state if the active keys aren't being held
            //(this is to prevent the key from repeating at 30 frames per second which would basically
            // make it impossible to play unless The Flash is playing or something
            if (newState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.H) & newState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.S)) { keyhit = false; }


            if (newState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H) & !keyhit)
            {
                keyhit = true; //we've registered this keypress, we're not accepting anymore until they let go!
                deck.deal_animated(cd_hand1);  //deal one card (and animated it) to the player's hand
                playerHadTurn = true;  //record that the player has made their turn (they will have to wait for the dealer's turn now!)
            }
            else if (newState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S) & !keyhit)
            {
                keyhit = true;
                gamestate = gs_standing; //once you stand you're done, and we'll just wait for the dealer now!
            }


        }

        //code for the dealer's move
        void dealerturn()
        {
            //grab the scores so we can plan our devious strategy
            int playerscore = deck.hand1SumBJ();
            int dealerscore = deck.hand2SumBJ();

            if (gamestate == gs_playing) //if the player is still playing
            {
                //if our score is less than the players we must play (unless we have 21 - in which case the next results update will declare us victorious!)
                if (dealerscore < 16 | dealerscore < playerscore & dealerscore != 21) { deck.deal_animated(cd_hand2); playerHadTurn = false; }//setup for player's turn
            }
            else if (gamestate == gs_standing) //if the player is done playing
            {
                //this is almost the same sort of thing, but the dealer keeps playing as many turns as required if the player is done
                if (dealerscore < playerscore & playerscore < 21 & dealerscore < 21) { deck.deal_animated(cd_hand2); playerHadTurn = false; }

            }


        }
        # endregion

        # region Draw
        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            
            cardBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // let the deck object draw whatever it needs (including calling each card's draw event)
            deck.Draw();

            //draw the scores
            string svalue = "   YOU " + Convert.ToString(deck.hand1SumBJ());
            gameFont.drawText(svalue, 320, 256, Color.White);
            svalue = "DEALER " + Convert.ToString(deck.hand2SumBJ());
            gameFont.drawText(svalue, 320, 128, Color.White);

            gameFont.drawText("DATA TRANSFER" , 1600, 90, Color.White);

            //if the game is over draw the results
            if (gamestate == gs_gameover)
            {
                flag = true;
                int dealer = deck.hand2SumBJ();
                int player = deck.hand1SumBJ();
                string result;
                if (dealer > 21)
                {
                    gameFont.drawText("DEALER BUST PRESS ENTER", 256, 400, Color.Red);
                    result = "DealerBust";
                }
                else if (player > 21)
                {
                    gameFont.drawText("PLAYER BUST PRESS ENTER", 256, 400, Color.Red);
                    result = "PlayerBust";
                }
                else if (player > dealer)
                {
                    gameFont.drawText("YOU WON PRESS ENTER", 256, 400, Color.Red);
                    result = "YouWon";
                }
                else if (dealer == 21 & player == 21)
                {
                    gameFont.drawText("TIE PRESS ENTER", 256, 400, Color.Red);
                    result = "Tie";
                }
                else
                {
                    gameFont.drawText("HOUSE WINS PRESS ENTER", 256, 400, Color.Red);
                    result = "DealerWon";
                }
                string transferString = result;
                arr = transferString.ToCharArray();
                if(location < arr.Length)
                {
                    binaryResult = ConvertToBinary(arr[location]);
                }
               
                //Console.WriteLine(arr[location].ToString());
                binaryResultLength = binaryResult.Length;

                for (int i = 0; i < binaryResultLength; i++)
                {
                    if (binaryResult[i] == '1')
                    {
                        if (st == 1)
                        {
                            //cardBatch.Draw(light, new Rectangle((1450), 210 + (i * 39), 10, 10), sourceRect, Color.Black);
                            cardBatch.Draw(light, new Rectangle((1450), 210 + (i * 20), 20, 20), sourceRect, Color.Black); //Seth's Testing
                        }
                        else
                            //cardBatch.Draw(light, new Rectangle((1450), 210 + (i * 39), 10, 10), sourceRect, Color.White);
                            cardBatch.Draw(light, new Rectangle((1450), 210 + (i * 20), 20, 20), sourceRect, Color.White); //Seth's Testing
                    }
                    if (binaryposition <= binaryResultLength - 1)
                    {
                        binaryposition++;
                    }

                }
                //cardBatch.Draw(light, new Rectangle(1650, 210, 10, 10), sourceRect, Color.White);
                cardBatch.Draw(light, new Rectangle(1550, 210, 10, 10), sourceRect, Color.White); //Seth's Testing

            }
            else //if the game is over draw my crappy instructions (hey my gamefont doesn't have brackets yet!)
            {
                gameFont.drawText("H IT OR S TAND", 264, 400, Color.Red);
            }

            if (flag == true)
            {
                if (location < arr.Length)
                {
                    binaryResult = ConvertToBinary(arr[location]);
                    binaryResultLength = binaryResult.Length;
                    //Console.WriteLine(binaryResult.ToString());
                    //Console.WriteLine(binaryResultLength.ToString());
                }
            }
            

            cardBatch.End();
            
            
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationServices.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            base.Draw(gameTime);
        }

        # endregion

        # region BinaryConversion Methods

        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();

            for (int i = 0; i < data.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }

            return Encoding.ASCII.GetString(byteList.ToArray());
        }

        public static string ConvertToBinary(char asciiString)
        {
            string result = string.Empty;

            result += Convert.ToString((int)asciiString, 2);

            return result;
        }

        #endregion

        #region Application Event Handlers

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: Enable audio, animations here

            //TODO: Optionally enable raw image here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: Optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: Disable audio, animations here

            //TODO: Disable raw image if it's enabled
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                    touchTarget = null;
                }
            }

            // Release unmanaged Resources.

            // Set large objects to null to facilitate garbage collection.

            base.Dispose(disposing);
        }

        #endregion
    }
}
