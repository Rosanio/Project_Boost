using Packages.Rider.Editor.UnitTesting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{

    [SerializeField] float rcsThrust = 150f;
    [SerializeField] float mainThrust = 1f;
    [SerializeField] AudioClip mainEngine = null;
    [SerializeField] AudioClip deathSound = null;
    [SerializeField] AudioClip levelCompleteSound = null;
    [SerializeField] ParticleSystem thrustParticles = null;
    [SerializeField] ParticleSystem deathParticles = null;
    [SerializeField] ParticleSystem levelCompleteParticles = null;

    new Rigidbody rigidbody;
    AudioSource audioSource;

    enum State {  Alive, Dying, Transcending }
    State state = State.Alive;

    private int currentLevelIndex;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        currentLevelIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            Thrust();
            Rotate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) return;

        switch(collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                LevelCompleted();
                break;
            default:
                LevelFailed();
                break;
        }
    }

    private void LevelCompleted()
    {
        currentLevelIndex += 1;
        state = State.Transcending;
        audioSource.Stop();
        thrustParticles.Stop();
        audioSource.PlayOneShot(levelCompleteSound);
        levelCompleteParticles.Play();
        Invoke(nameof(LoadNextScene), 1f);
    }

    private void LevelFailed()
    {
        currentLevelIndex = 0;
        state = State.Dying;
        audioSource.Stop();
        thrustParticles.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke(nameof(LoadNextScene), 1f);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(currentLevelIndex);
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            thrustParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidbody.AddRelativeForce(Vector3.up * mainThrust);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        if (!thrustParticles.isPlaying)
        {
            thrustParticles.Play();
        }
    }

    private void Rotate()
    {
        // Disable physics rotation
        rigidbody.freezeRotation = true;

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward, rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward, rotationThisFrame);
        }

        // Resume physics rotation
        rigidbody.freezeRotation = false;
    }
}
