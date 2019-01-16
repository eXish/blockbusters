using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hexTile : MonoBehaviour
{
		public KMSelectable selectable;
		public TextMesh containedLetter;
		public bool legalTile;
		public int tileNumber;
		public List<int> legalNextTile = new List<int>();
		public bool tileTaken;
		public bool finishingTile;
		public Renderer background;
}
