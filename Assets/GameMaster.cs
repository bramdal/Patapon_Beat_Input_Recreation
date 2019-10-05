using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{

    AudioSource[] audioSources;
    AudioSource masterBeat;
    AudioSource commandMutedBeat;
    AudioSource beatMissSigh;
    AudioSource beatSkipSigh;

    AudioSource drumTop;
    AudioSource drumRight;
    AudioSource drumBottom;
    AudioSource drumLeft;

    [Header("Public references")]
    public TeamController teamController;
    bool allowedToBeat;
    //beat track variables
    [Header("Beat timing variables")]
    [Range(0, 120)]
    public float beatsPerMinute = 80;
    [Range(0, 1)]
    public float errorMarginTime = .3f;

    
    int[] commandType;
    int commandCount = 0;
    int inactiveBeatCount = 0;  //how many beats after command are inactive

    //measure how long an active beat time has no input
    float beatFallTime;
    
    //count how long beat is active without an input
    private float beatActiveTime = 0f; 
    new private bool enabled;
    public bool hasBeatInput{
        get{
            return enabled;
        }
        set{
            enabled = value;
            if(!enabled)
                beatActiveTime = Time.time;
        }
    }
    
    bool lastBeatHasInput = true;   //true means no, false means yes, used with offset along with hasBeatInput

    //ui flash colour variables
    Color flashColor = new Color(255f, 255f, 255f, 1);
    [Header("Sprite Display Variables")]
    public Image drumTopSprite;
    public Image drumRightSprite;
    public Image drumBottomSprite;
    public Image drumLeftSprite;
    Image currentDrumSprite = null;
    [Range(0.25f , 0.5f)]
    public float spriteFlashTime = 0.5f;

    //fever variables
    bool fever;
    float feverTimeHold;
    public Image feverSprite;
    


    void Start()
    {
        allowedToBeat = true;
        hasBeatInput = false;

        inactiveBeatCount = 0;

        float invokeTime  = 60f / beatsPerMinute;
        teamController.secondsToBeats = (int)(invokeTime * 4);

        commandType = new int[4]{0, 0, 0, 0};

        audioSources = GetComponents<AudioSource>();
        masterBeat = audioSources[0];
        commandMutedBeat = audioSources[1];
        beatMissSigh = audioSources[2];
        beatSkipSigh = audioSources[3];
        
        drumTop = audioSources[4];
        drumRight = audioSources[5];
        drumBottom = audioSources[6];
        drumLeft = audioSources[7];
        
        beatFallTime = errorMarginTime;

        InvokeRepeating("PlayMasterBeat", errorMarginTime/2f, invokeTime);
        InvokeRepeating("AllowBeat", 0f, invokeTime);
    }

   
    void Update()
    {
        beatFallTime -= Time.deltaTime;
            if(beatFallTime <0f){
                allowedToBeat = false;
                
            if(commandType[3] != 0){
                bool commandMatched = SetInput(commandType);
                if(commandMatched){
                    commandCount++;
                    inactiveBeatCount = 4;      //4 beats after input are inactive
                }       
                else{
                    inactiveBeatCount = 0; 
                    commandCount = 0;
                }       
                Array.Clear(commandType, 0, commandType.Length);
            }    
        }    

        if(allowedToBeat && hasBeatInput && Input.anyKeyDown){      //double beat per master beat
                print("double beat not allowed");
                hasBeatInput = false;
                lastBeatHasInput = true;
                Array.Clear(commandType, 0, commandType.Length);
        }

        GetDrumInputs();
           
        if(!allowedToBeat && Input.anyKeyDown){                     //mistiming beat with master beat
            beatMissSigh.Play();
            Array.Clear(commandType, 0, commandType.Length);
            commandCount = 0;
        }  
        
        if(inactiveBeatCount >0 && Input.anyKeyDown){               //interrupting command
            Array.Clear(commandType, 0, commandType.Length);
            commandCount = 0;
            //do physical motion stop here
        }
    
        
        if(Time.time - beatActiveTime >= errorMarginTime && lastBeatHasInput && allowedToBeat){      //skipping a master beat
            lastBeatHasInput = true;
            Array.Clear(commandType, 0, commandType.Length);
            
        }

       if(currentDrumSprite != null){
           Image temporaryReference = currentDrumSprite;
           temporaryReference.color = Color.Lerp(currentDrumSprite.color, Color.clear, spriteFlashTime);
       }


        //continuos beats required to maintain fever
        if(commandCount >=4 ){
            fever = true;
            feverSprite.gameObject.SetActive(true);
        }    

        if(inactiveBeatCount>=0){
            feverTimeHold = Time.time;
        }
        if(Time.time - feverTimeHold >= ((errorMarginTime)*2) + 1f && fever ){    
            commandCount = 0;
            fever = false;
            feverSprite.gameObject.SetActive(false);
        }    
    }
    

    void AllowBeat(){
        beatFallTime = errorMarginTime;

        if(inactiveBeatCount == 0)
            teamController.resetSpritesToIdle();
            
        allowedToBeat = true; 
        
        if(hasBeatInput){
            hasBeatInput = false;
        }
    }
    void PlayMasterBeat(){
        if((inactiveBeatCount--)>0){
            commandMutedBeat.Play();
        }      
        else
            masterBeat.Play();    
    }

    bool SetInput(int[] commandType){
        bool commandMatched = teamController.GetInput(commandType);
        return commandMatched;
    }

    void GetDrumInputs(){
        if(allowedToBeat){
            if(commandType[0] == 0){
                if(Input.GetButtonDown("Left")){
                    commandType[0] = 1;
                    hasBeatInput = true;
                    drumLeft.Play();
                    currentDrumSprite = drumLeftSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Right")){ 
                    commandType[0] = 2;
                    hasBeatInput = true;
                    drumRight.Play();
                    currentDrumSprite = drumRightSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Top")){ 
                    commandType[0] = 3; 
                    hasBeatInput = true;
                    drumTop.Play();
                    currentDrumSprite = drumTopSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Down")){ 
                    commandType[0] = 4;            
                    hasBeatInput = true;
                    drumBottom.Play();
                    currentDrumSprite = drumBottomSprite;
                    currentDrumSprite.color = flashColor;
                    }
            }
            else if(commandType[1] == 0){
                if(Input.GetButtonDown("Left")){
                    commandType[1] = 1;
                    hasBeatInput = true;
                    drumLeft.Play();
                    currentDrumSprite = drumLeftSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Right")){ 
                    commandType[1] = 2;
                    hasBeatInput = true;
                    drumRight.Play();
                    currentDrumSprite = drumRightSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Top")){ 
                    commandType[1] = 3; 
                    hasBeatInput = true;
                    drumTop.Play();
                    currentDrumSprite = drumTopSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Down")){ 
                    commandType[1] = 4;  
                    hasBeatInput = true;
                    drumBottom.Play();
                    currentDrumSprite = drumBottomSprite;
                    currentDrumSprite.color = flashColor;
                }
            }
            else if(commandType[2] == 0){
                if(Input.GetButtonDown("Left")){
                    commandType[2] = 1;
                    hasBeatInput = true;
                    drumLeft.Play();
                    currentDrumSprite = drumLeftSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Right")){ 
                    commandType[2] = 2;
                    hasBeatInput = true;
                    drumRight.Play();
                    currentDrumSprite = drumRightSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Top")){ 
                    commandType[2] = 3; 
                    hasBeatInput = true;
                    drumTop.Play();
                    currentDrumSprite = drumTopSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Down")){ 
                    commandType[2] = 4;  
                    hasBeatInput = true;
                    drumBottom.Play();
                    currentDrumSprite = drumBottomSprite;
                    currentDrumSprite.color = flashColor;
                }
            }
            else if(commandType[3] == 0){
                if(Input.GetButtonDown("Left")){
                    commandType[3] = 1;
                    hasBeatInput = true;
                    drumLeft.Play();
                    currentDrumSprite = drumLeftSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Right")){ 
                    commandType[3] = 2;
                    hasBeatInput = true;
                    drumRight.Play();
                    currentDrumSprite = drumRightSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Top")){ 
                    commandType[3] = 3; 
                    hasBeatInput = true;
                    drumTop.Play();
                    currentDrumSprite = drumTopSprite;
                    currentDrumSprite.color = flashColor;
                }
                else if(Input.GetButtonDown("Down")){ 
                    commandType[3] = 4;
                    hasBeatInput = true;
                    drumBottom.Play();
                    currentDrumSprite = drumBottomSprite;
                    currentDrumSprite.color = flashColor;
                }
            }
        }
        lastBeatHasInput = !hasBeatInput;
    }    
}
