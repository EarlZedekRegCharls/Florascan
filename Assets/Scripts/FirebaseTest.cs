using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseTest : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
        // Step 1: Check that Firebase dependencies are available before doing anything
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("[FirebaseTest] Firebase dependencies OK. Initializing Firestore...");
                db = FirebaseFirestore.DefaultInstance;
                WriteTestData();
            }
            else
            {
                Debug.LogError($"[FirebaseTest] Could not resolve Firebase dependencies: {status}");
            }
        });
    }

    void WriteTestData()
    {
        // Step 2: Write a test document to a "test" collection
        DocumentReference docRef = db.Collection("test").Document("connectionCheck");

        Dictionary<string, object> testData = new Dictionary<string, object>
        {
            { "message", "Hello from Unity!" },
            { "timestamp", System.DateTime.UtcNow.ToString() }
        };

        docRef.SetAsync(testData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[FirebaseTest] Write successful! Now reading it back...");
                ReadTestData();
            }
            else
            {
                Debug.LogError($"[FirebaseTest] Write failed: {task.Exception}");
            }
        });
    }

    void ReadTestData()
    {
        // Step 3: Read the same document back to confirm round-trip works
        DocumentReference docRef = db.Collection("test").Document("connectionCheck");

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log("[FirebaseTest] SUCCESS! Read back: " + snapshot.GetValue<string>("message"));
                    Debug.Log("[FirebaseTest] Firebase connection is fully working end-to-end.");
                }
                else
                {
                    Debug.LogWarning("[FirebaseTest] Document does not exist after write. Something's off.");
                }
            }
            else
            {
                Debug.LogError($"[FirebaseTest] Read failed: {task.Exception}");
            }
        });
    }
}
