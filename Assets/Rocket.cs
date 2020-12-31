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
    [SerializeField] float levelLoadDelay = 1f;

    new Rigidbody rigidbody;
    AudioSource audioSource;

    enum State {  Alive, Dying, Transcending }
    State state = State.Alive;

    private int currentLevelIndex;
    private bool collisionDetectionActive = true;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            Thrust();
            Rotate();
        }
        if (Debug.isDebugBuild)
        {
            ProcessDebugKeys();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !collisionDetectionActive) return;

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
        currentLevelIndex = GetNextLevelIndex();
        state = State.Transcending;
        audioSource.Stop();
        thrustParticles.Stop();
        audioSource.PlayOneShot(levelCompleteSound);
        levelCompleteParticles.Play();
        Invoke(nameof(LoadNextScene), levelLoadDelay);
    }

    private int GetNextLevelIndex()
    {
        return currentLevelIndex == SceneManager.sceneCountInBuildSettings - 1 ? 0 : currentLevelIndex + 1;
    }

    private void LevelFailed()
    {
        currentLevelIndex = currentLevelIndex == 0 ? 0 : currentLevelIndex - 1;
        state = State.Dying;
        audioSource.Stop();
        thrustParticles.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke(nameof(LoadNextScene), levelLoadDelay);
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

    private void ProcessDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            currentLevelIndex = GetNextLevelIndex();
            LoadNextScene();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionDetectionActive = !collisionDetectionActive;
        }
    }
}
