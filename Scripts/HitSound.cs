using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSound : MonoBehaviour
{
    public AudioSource soundToPlay;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer != 6)
        {
            soundToPlay.Stop();
            soundToPlay.pitch = Random.Range(0.8f, 1.2f);
            soundToPlay.Play();
        }
        
    }
}
