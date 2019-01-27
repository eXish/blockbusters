using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class blockbustersScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public hexTile[] tiles;
    public string[] alphabet;
    public Material[] tileMats;
    public Renderer background;

    private List<int> chosenLetters = new List<int>();
    public List<string> legalLetters = new List<string>();
    public List<string> illegalLetters = new List<string>();

    private List<int> selectedIllLetters = new List<int>();

    private hexTile lastPressedTile;


    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (hexTile tile in tiles)
        {
            hexTile pressedTile = tile;
            tile.selectable.OnInteract += delegate () { TilePress(pressedTile); return false; };
        }
    }


    void Start()
    {
        DetermineLegalLetters();
        SetStartPoint();
    }

    void SetStartPoint()
    {
        foreach(hexTile tile in tiles)
        {
            tile.GetComponent<Renderer>().material = tileMats[0];
            int index = UnityEngine.Random.Range(0,24);
            while(chosenLetters.Contains(index))
            {
                index = UnityEngine.Random.Range(0,24);
            }
            chosenLetters.Add(index);
            tile.containedLetter.text = alphabet[index];
        }
        chosenLetters.Clear();

        for(int i = 0; i <= 3; i++)
        {
            int illLetter = UnityEngine.Random.Range(0, illegalLetters.Count());
            while(selectedIllLetters.Contains(illLetter))
            {
                illLetter = UnityEngine.Random.Range(0, illegalLetters.Count());
            }
            selectedIllLetters.Add(illLetter);
            tiles[i].containedLetter.text = illegalLetters[illLetter];
        }
        int index2 = UnityEngine.Random.Range(0,4);
        int letter = UnityEngine.Random.Range(0, legalLetters.Count());
        tiles[index2].containedLetter.text = legalLetters[letter];
        tiles[index2].legalTile = true;
        selectedIllLetters.Clear();
        Debug.LogFormat("[Blockbusters #{0}] Welcome to Blockbusters! Your first tile is {1}.", moduleId, tiles[index2].containedLetter.text);
    }

    void SetTileTwo()
    {
        foreach(hexTile tile in tiles)
        {
            if(!tile.tileTaken)
            {
                tile.GetComponent<Renderer>().material = tileMats[0];
                int index = UnityEngine.Random.Range(0,24);
                while(chosenLetters.Contains(index))
                {
                    index = UnityEngine.Random.Range(0,24);
                }
                chosenLetters.Add(index);
                tile.containedLetter.text = alphabet[index];
            }
        }
        chosenLetters.Clear();

        for(int i = 0; i < lastPressedTile.legalNextTile.Count(); i++)
        {
            if(!tiles[lastPressedTile.legalNextTile[i]].tileTaken)
            {
                int illLetter = UnityEngine.Random.Range(0, illegalLetters.Count());
                while(selectedIllLetters.Contains(illLetter))
                {
                    illLetter = UnityEngine.Random.Range(0, illegalLetters.Count());
                }
                selectedIllLetters.Add(illLetter);
                tiles[lastPressedTile.legalNextTile[i]].containedLetter.text = illegalLetters[illLetter];
            }
        }
        int index2 = UnityEngine.Random.Range(0, lastPressedTile.legalNextTile.Count());
        while(tiles[lastPressedTile.legalNextTile[index2]].tileTaken)
        {
            index2 = UnityEngine.Random.Range(0,lastPressedTile.legalNextTile.Count());
        }
        int letter = UnityEngine.Random.Range(0, legalLetters.Count());
        tiles[lastPressedTile.legalNextTile[index2]].containedLetter.text = legalLetters[letter];
        tiles[lastPressedTile.legalNextTile[index2]].legalTile = true;
        selectedIllLetters.Clear();
        Debug.LogFormat("[Blockbusters #{0}] Your next tile is {1}.", moduleId, tiles[lastPressedTile.legalNextTile[index2]].containedLetter.text);
    }

    void TilePress(hexTile tile)
    {
        if(moduleSolved || tile.tileTaken)
        {
            return;
        }
        GetComponent<KMSelectable>().AddInteractionPunch();
        if(tile.legalTile && tile.finishingTile)
        {
            Debug.LogFormat("[Blockbusters #{0}] You pressed {1}. That is correct. Module disarmed.", moduleId, tile.containedLetter.text);
            GetComponent<KMBombModule>().HandlePass();
            Audio.PlaySoundAtTransform("theme", transform);
            lastPressedTile = tile;
            tile.containedLetter.text = "";
            tile.tileTaken = true;
            foreach(hexTile allTaken in tiles)
            {
                if(allTaken.tileTaken)
                {
                    allTaken.GetComponent<Renderer>().material = tileMats[2];
                }
            }
            foreach(hexTile allTiles in tiles)
            {
                allTiles.legalTile = false;
            }
            moduleSolved = true;
            StartCoroutine(Disco());
        }
        else if(tile.legalTile)
        {
            Audio.PlaySoundAtTransform("buzzer", transform);
            lastPressedTile = tile;
            Debug.LogFormat("[Blockbusters #{0}] You pressed {1}. That is correct.", moduleId, tile.containedLetter.text);
            foreach(hexTile allTaken in tiles)
            {
                if(allTaken.tileTaken)
                {
                    allTaken.GetComponent<Renderer>().material = tileMats[2];
                }
            }
            tile.GetComponent<Renderer>().material = tileMats[1];
            tile.containedLetter.text = "";
            tile.tileTaken = true;
            foreach(hexTile allTiles in tiles)
            {
                allTiles.legalTile = false;
            }
            SetTileTwo();
        }
        else
        {
            Debug.LogFormat("[Blockbusters #{0}] Strike! You pressed {1}. That is incorrect. Resetting grid.", moduleId, tile.containedLetter.text);
            GetComponent<KMBombModule>().HandleStrike();
            foreach(hexTile allTiles in tiles)
            {
                allTiles.legalTile = false;
                allTiles.tileTaken = false;
            }
            Start();
        }
    }

    IEnumerator Disco()
    {
        yield return new WaitForSeconds(1.2f);
        int flashCount = 0;
        Vector3 temp = new Vector3(0,-0.05f,0);
        Vector3 temp2 = new Vector3(0,0f,0);
        foreach(hexTile tile in tiles)
        {
            if(tile.tileTaken)
            {
                tile.background.transform.localPosition = temp;
            }
        }
        while(flashCount < 2)
        {
            foreach(hexTile tile in tiles)
            {
                if(tile.tileTaken)
                {
                    tile.GetComponent<Renderer>().material = tileMats[4];
                    tile.background.material = tileMats[4];
                }
                background.material = tileMats[4];
            }
            yield return new WaitForSeconds(0.3f);foreach(hexTile tile in tiles)
            {
                if(tile.tileTaken)
                {
                    tile.GetComponent<Renderer>().material = tileMats[2];
                    tile.background.material = tileMats[2];
                }
                background.material = tileMats[2];
            }
            yield return new WaitForSeconds(0.6f);
            flashCount++;
        }
        foreach(hexTile tile in tiles)
        {
            if(tile.tileTaken)
            {
                tile.GetComponent<Renderer>().material = tileMats[4];
                tile.background.material = tileMats[4];
            }
            background.material = tileMats[4];
        }
        yield return new WaitForSeconds(0.3f);
        foreach(hexTile tile in tiles)
        {
            if(tile.tileTaken)
            {
                tile.background.transform.localPosition = temp2;
            }
        }
        while(flashCount < 23)
        {
            foreach(hexTile tile in tiles)
            {
                tile.containedLetter.text = "";
                int index = UnityEngine.Random.Range(0,5);
                tile.GetComponent<Renderer>().material = tileMats[index];
                tile.background.material = tileMats[4];
            }
            int index2 = UnityEngine.Random.Range(0,4);
            background.material = tileMats[index2];
            yield return new WaitForSeconds(0.1f);
            flashCount++;
        }
        foreach(hexTile tile in tiles)
        {
            tile.GetComponent<Renderer>().material = tileMats[2];
        }
        background.material = tileMats[4];

    }

    void DetermineLegalLetters()
    {
        legalLetters.Clear();
        illegalLetters.Clear();
        int unlit = Bomb.GetOffIndicators().Count();
        int lit = Bomb.GetOnIndicators().Count();
        int batts = Bomb.GetBatteryCount();
        int dBatts = Bomb.GetBatteryCount(Battery.D);
        int aaBatts = batts - dBatts;
        int RJ = Bomb.GetPortCount(Port.RJ45);
        int RCA = Bomb.GetPortCount(Port.StereoRCA);
        int PS2 = Bomb.GetPortCount(Port.PS2);
        int DVI = Bomb.GetPortCount(Port.DVI);
        int battHolders = Bomb.GetBatteryHolderCount();
        int portPlates = Bomb.GetPortPlates().Count();
        int arrayInc = 0;

        if(unlit > batts)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(RJ > 2)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(RCA == 0)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(battHolders == 3)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.IsIndicatorOn("FRK"))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(PS2 > 0)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(portPlates < 2)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(dBatts == 3)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if (Bomb.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U'))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.GetPortPlates().Any(x => x.Contains(Port.Parallel.ToString()) && x.Contains(Port.Serial.ToString())))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.IsIndicatorOff("CAR"))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(portPlates + battHolders < 4)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(batts == 5)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if((Bomb.GetSerialNumberNumbers().Last() % 2) == 1)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(DVI > dBatts)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.IsIndicatorOn("BOB"))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if (Bomb.GetSerialNumberLetters().All(x => x != 'A' && x != 'E' && x != 'I' && x != 'O' && x != 'U'))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(aaBatts == 4)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.GetSerialNumberLetters().Count() < portPlates)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(portPlates > 1)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.GetSerialNumberNumbers().Sum() > 17)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.IsIndicatorOff("IND"))
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(lit > unlit)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;

        if(Bomb.GetSerialNumberNumbers().Count() > lit)
        {
            legalLetters.Add(alphabet[arrayInc]);
        }
        else
        {
            illegalLetters.Add(alphabet[arrayInc]);
        }
        arrayInc++;
    }


    public void PressButton()
    {
        if(moduleSolved)
        {
            return;
        }
    }

    int CharacterToIndex(char character)
    {
        return character >= 'a' ? character - 'a' : character - '1';
    }

    bool InRange(int number, int min, int max)
    {
        return max >= number && number >= min;
    }

    public string TwitchHelpMessage = "Press a tile using !{0} B5. Tiles are specified by column then row.";

    public IEnumerator ProcessTwitchCommand(string inputCommand)
    {
        inputCommand = Regex.Replace(inputCommand.ToLowerInvariant(), @"(\W|_|^(press|submit|click|answer))", "");
        if (inputCommand.Length != 2) yield break;

        int column = CharacterToIndex(inputCommand[0]);
        int row = CharacterToIndex(inputCommand[1]);

        if (InRange(column, 0, 4) && InRange(row, 0, 4) && (row < 4 || column % 2 == 1))
        {
            yield return null;
            int index = Enumerable.Range(0, column).Select(n => (n % 2 == 0) ? 4 : 5).Sum() + row;
            tiles[index].selectable.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
