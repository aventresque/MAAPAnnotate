using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.UI; //Required when Using UI elements

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Handle the translation from speech to text
public class DictationScript : MonoBehaviour {

    public Text mainText;

    private DictationRecognizer dictationRecognizer;
    
    //event handler: will trigger when the DictationRecognizer class raises the DictationHypothesis event
    private void OnDictationHypothesis(string textRecognized)
    {
        //write the dictation hypothesis in a public text chosen
        mainText.text = textRecognized;
        Debug.LogFormat("Dictation hypothesis: {0}", textRecognized);
    }

    //start and suscribe dictation
    public void StartDictation()
    {
        if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            //shutdown the phraseRecognition = grammarRecognizer or keywordReconizer
            PhraseRecognitionSystem.Shutdown();
        }

        dictationRecognizer = new DictationRecognizer();
        Debug.Log("Start the dictation");
        //the class DictationScript suscribes to the event DictaitonHypothesis
        dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
        dictationRecognizer.Start();
    }

    public void StopDictation()
    {
        //if we have already been in startDictation
        if (dictationRecognizer != null)
        {
            //unsuscribe to the different events you have suscribe too
            dictationRecognizer.DictationHypothesis -= OnDictationHypothesis;
            //release the ressource the dictationRecognizer used
            dictationRecognizer.Dispose();
            dictationRecognizer.Stop();
            Debug.Log("Stop the dictation");

            if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Stopped)
            {
                //restart the phraseRecognition = grammarRecognizer or keywordReconizer
                PhraseRecognitionSystem.Restart();
            }
        }
    }
}
