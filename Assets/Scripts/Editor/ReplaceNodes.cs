using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceNodes : MonoBehaviour
{

    [UnityEditor.MenuItem("Moon/Replace Nodes with node Prefabs")]
    public static void ReplaceAllNodesWithPrefabs()
    {
        var actor = FindObjectOfType<Tools>();
        Dictionary<Node, Node> replacement = new Dictionary<Node, Node>();
        var map = actor.map;
        for (int i = 0; i < map.nodes.Length; i++)
        {
            var node = map.nodes[i];
            GameObject newNode = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(actor.nodePrefab, node.transform.parent);
            newNode.transform.position = node.transform.position;
            newNode.transform.rotation = node.transform.rotation;
            newNode.name = node.gameObject.name;
            replacement[node] = newNode.GetComponent<Node>();
        }
        for (int i = 0; i < map.edges.Length; i++)
        {
            map.edges[i].start = replacement[map.edges[i].start];
            map.edges[i].end = replacement[map.edges[i].end];
        }
        for (int i = 0; i < map.nodes.Length; i++)
        {
            var oldNode = map.nodes[i];
            map.nodes[i] = replacement[oldNode];
            DestroyImmediate(oldNode.gameObject);
        }
    }
}
