using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class KeywordScript : MonoBehaviour
{
    [SerializeField]
    string[] _activate;
    [SerializeField]
    string[] _menu;

    [SerializeField]
    bool _activateFlag = false;

    private KeywordRecognizer _activateRecognizer;
    private KeywordRecognizer _menuRecognizer;

    void Start()
    {
        _activateRecognizer = new KeywordRecognizer(_activate);
        _activateRecognizer.OnPhraseRecognized += OnActivate;
        _activateRecognizer.Start();

        _menuRecognizer = new KeywordRecognizer(_menu);
        _menuRecognizer.OnPhraseRecognized += OnMenu;

    }

    private void OnActivate(PhraseRecognizedEventArgs args)
    {
        _activateFlag = true;
        Debug.Log("Acitvate");
        _menuRecognizer.Start();
        StartCoroutine("MenuTimer");
    }


    private void OnMenu(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Menu");
    }

    IEnumerator MenuTimer()
    {
        yield return new WaitForSeconds(10.0f);
        _menuRecognizer.Stop();
        _activateFlag = false;
    }
}