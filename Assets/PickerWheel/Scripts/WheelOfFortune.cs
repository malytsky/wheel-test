using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PickerWheel.Scripts
{
   public class WheelOfFortune
   {
      private const int Pieces = 16;
      private const float FullRound = 360f;

      public event Func<GameObject, Vector3, Quaternion, Transform, GameObject> OnInstantiate;
      public event Action<GameObject> OnDestroy;
      public event Action OnSpinEndEvent;
      
      private bool _isSpinning;
      private readonly Vector2 _pieceMinSize = new(81f, 146f);
      private readonly Vector2 _pieceMaxSize = new(144f, 213f);

      private float _pieceSector;
      private float _halfSector;
      private float _halfSectorWithPaddings;
      
      private double _accumulatedWeight;
      private int _score;

      private List<int> _values;

      private Transform _wheelCircle;
      private GameObject _wheelPiecePrefab;
      
      private GameObject _linePrefab;
      private Transform _linesParent;
      private Transform _wheelPiecesParent;
      private readonly int _spinDuration;
      private AudioSource _audioSource1;
      private AudioSource _audioSource2;
      private Text _scoreText;

      public WheelOfFortune(Transform wheelCircle, GameObject wheelPiecePrefab, GameObject linePrefab, int spinDuration, AudioSource audioSource1, AudioSource audioSource2, Text scoreText)
      {
         _wheelCircle = wheelCircle;
         _wheelPiecePrefab = wheelPiecePrefab;
         _linePrefab = linePrefab;
         _spinDuration = spinDuration;
         _audioSource1 = audioSource1;
         _audioSource2 = audioSource2;
         _scoreText = scoreText;
         
         _linesParent = wheelCircle.Find(DefaultVariables.Lines);
         _wheelPiecesParent = wheelCircle.Find(DefaultVariables.WheelPieces);
      }
      
      public void Create()
      {
         _pieceSector = FullRound / Pieces;
         _halfSector = _pieceSector / 2f;
         _halfSectorWithPaddings = _halfSector - _halfSector / 4f;
         _score = 0;

         Generate();
      }

      private void Generate()
      {
         SetScore();
         _wheelPiecePrefab = OnInstantiate?.Invoke(_wheelPiecePrefab, _wheelPiecesParent.position, Quaternion.identity, _wheelPiecesParent);

         if (_wheelPiecePrefab != null)
         {
            var rectTransform = _wheelPiecePrefab.transform.Find(DefaultVariables.PieceHolder).GetComponent<RectTransform>();
            var pieceWidth = Mathf.Lerp(_pieceMinSize.x, _pieceMaxSize.x, 1f - Pieces);
            var pieceHeight = Mathf.Lerp(_pieceMinSize.y, _pieceMaxSize.y, 1f - Pieces);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);
         }

         _values = new List<int>();

         for (var i = 0; i < Pieces; i++)
         {
            SetWheelCount();
            DrawPiece(i);
         }
         
         OnDestroy?.Invoke(_wheelPiecePrefab);
      }

      private void SetWheelCount()
      {
         var num = Random.Range(10, 1000) * 100;
         
         // ReSharper disable once ForCanBeConvertedToForeach
         for (var i = 0; i < _values.Count; i++)
         {
            if (num > _values[i] + 1000 || num < _values[i] - 1000) continue;
            Debug.Log("Replacing the value " + num + " close to " + _values[i] + ", according to task condition");
            SetWheelCount();
         }
         _values.Add(num);
      }

      private void DrawPiece(int index) {
         var piece = OnInstantiate?.Invoke(_wheelPiecePrefab, _wheelPiecesParent.position, Quaternion.identity, _wheelPiecesParent);
         if (piece == null) return;
         var pieceHolder = piece.transform.Find(DefaultVariables.PieceHolder);
         pieceHolder.Find(DefaultVariables.Amount).GetComponent<Text>().text = _values[index].ToString();

         //Line
         var transform = OnInstantiate?.Invoke(_linePrefab, _linesParent.position, Quaternion.identity, _linesParent).transform;
         var position = _wheelPiecesParent.position;
         if (transform != null) transform.RotateAround(position, Vector3.back, (_pieceSector * index) + _halfSector);
         pieceHolder.RotateAround(position, Vector3.back, _pieceSector * index);
      }

      public void Spin(int value = -1) // if value get from server
      {
         if (_isSpinning) return;
         _isSpinning = true;

         var index = value > 0 ? value : Random.Range(0, Pieces);
         var angle = -(_pieceSector * index);
         var rightOffset = angle -_halfSectorWithPaddings % FullRound;
         var leftOffset = angle + _halfSectorWithPaddings % FullRound;
         var randomAngle = Random.Range(leftOffset, rightOffset);
         var targetRotation = Vector3.back * (randomAngle + 2 * FullRound * _spinDuration);
         
         float currentAngle;
         var prevAngle = currentAngle = _wheelCircle.eulerAngles.z;
         var isIndicatorOnTheLine = false;

         _wheelCircle
            .DORotate(targetRotation, _spinDuration)
            .SetEase(Ease.InOutQuart)
            .OnUpdate(() => {
               var diff = Mathf.Abs(prevAngle - currentAngle);
               if (diff >= _halfSector) {
                  if (isIndicatorOnTheLine) {
                     _audioSource1.PlayOneShot(_audioSource1.clip);
                  }
                  prevAngle = currentAngle;
                  isIndicatorOnTheLine = !isIndicatorOnTheLine;
               }
               currentAngle = _wheelCircle.eulerAngles.z;
            })
            .OnComplete(() =>
            {
               _score += _values[index];
               //pattern Adapter
               SetScore(FormatValues.FormatScore(_score));
               _audioSource2.PlayOneShot(_audioSource2.clip);
               _isSpinning = false;
               OnSpinEndEvent?.Invoke();
            });
      }

      private void SetScore(string value = "0")
      {
         _scoreText.text = DefaultVariables.Score + value;
      }

      
      public void CheckRecord()
      {
         //Save score if it is more than record 
         var record = PlayerPrefs.GetInt(DefaultVariables.TextToSaveRecord);
         if (_score > record)
         {
            PlayerPrefs.SetInt(DefaultVariables.TextToSaveRecord, _score);
         }
      }
      
      public void SaveCurrentResult()
      {
         GameVariables.LastResult = _score;
      }

      public void Clear()
      {
         OnInstantiate = null;
         OnDestroy = null;
         OnSpinEndEvent = null;
         _wheelPiecePrefab = null;
         _values = null;
         _wheelCircle = null;
         _linePrefab = null;
         _audioSource1 = null;
         _audioSource2 = null;
         _scoreText = null;

         _linesParent = null;
         _wheelPiecesParent = null;
      }
   }
}
