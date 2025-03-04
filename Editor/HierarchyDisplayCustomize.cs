using UnityEngine;
using UnityEditor;

public class HierarchyDisplayCustomize
{
    private static bool isCustomizationEnabled = true; // デフォルトで有効
    private static Texture2D backgroundTexture;
    private static bool isEditorOnly;
    private static bool shouldRepaint = false; // 再描画のフラグ

    private static Color defBackColor = new Color(1.0f, 0.5f, 0.5f, 0.7f);

    /// <summary>
    /// HierarchyDisplayCustomize クラスの初期化メソッドです。
    /// エディターがロードされた際に呼び出され、カスタマイズを設定します。
    /// </summary>
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        EditorApplication.hierarchyChanged += OnHierarchyChanged; // シーンヒエラルキーが変更された際のイベント
        backgroundTexture = GetBackgroundTexture(defBackColor); // 初回生成
        
    }

    /// <summary>
    /// シーンヒエラルキーが変更されたときに呼び出され、再描画をトリガーします。
    /// </summary>
    private static void OnHierarchyChanged()
    {
        // シーンヒエラルキーが変更されたときに再描画をトリガー
        shouldRepaint = true;
    }

    /// <summary>
    /// ヒエラルキーウィンドウ内の各アイテムの表示に関するカスタマイズを行います。
    /// これは再描画が必要な場合に実行され、オブジェクトのタグやアクティブ状態に基づいて表示を変更します。
    /// </summary>
    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        // 機能を無効にしている場合は何もしない
        if (!isCustomizationEnabled)
        {
            return; 
        }

        if (shouldRepaint)
        {
            // 再描画が必要な場合のみ処理を実行
            shouldRepaint = false;
            RepaintHierarchyWindow();
        }

        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        //Sceneなどオブジェクトが無い場合は終了
        if (gameObject == null)
        {
            return;
        }

        //オブジェクトがEditorOnlyタグがどうかで処理分け
        isEditorOnly = gameObject.CompareTag("EditorOnly");

        if (isEditorOnly)
        {
            HandleEditorOnlyItem(selectionRect, gameObject);
        }
        else
        {
            //現状専用の処理は無し
            HandleNonEditorOnlyItem(selectionRect, gameObject);
        }

        //EditorOnlyタグ切り替えボタンを描画
        HandleEditorOnlyButton(selectionRect, gameObject);

        //Active切り替えチェックボックスを描画
        HandleActiveToggle(selectionRect, gameObject);
    }
    
    /// <summary>
    /// このメソッドは変更通知があった場合に呼び出される
    /// </summary>
    private static void RepaintHierarchyWindow()
    {
        EditorApplication.RepaintHierarchyWindow();
    }

    /// <summary>
    /// "EditorOnly" タグを持つゲームオブジェクトの表示を処理します。
    /// </summary>
    private static void HandleEditorOnlyItem(Rect selectionRect, GameObject gameObject)
    {
        // ゲームオブジェクトが非アクティブの場合、フォントカラーを白色と灰色の中間色に変更
        bool isInactive = !gameObject.activeInHierarchy;
        Color textColor = isInactive ? new Color(0.75f, 0.75f, 0.75f) : Color.white;

        // オブジェクト名の表示位置
        Rect nameLabelRect = new Rect(selectionRect.x + 18, selectionRect.y, selectionRect.width, selectionRect.height);
  
        // オブジェクト名と背景色を表示
        GUIStyle backgroundStyle = GetBackgroundStyle();
        EditorGUI.LabelField(nameLabelRect, GUIContent.none, backgroundStyle);
        EditorGUI.LabelField(nameLabelRect, gameObject.name, new GUIStyle { normal = { textColor = textColor } });

    }

    /// <summary>
    /// "EditorOnly" タグを持たないゲームオブジェクトの表示を処理します。
    /// </summary>
    private static void HandleNonEditorOnlyItem(Rect selectionRect, GameObject gameObject)
    {
        // Handle non-EditorOnly items
    }

    /// <summary>
    /// "EditorOnly"と"Untagged"を切り替えるボタンを処理します。
    /// </summary>
    private static void HandleEditorOnlyButton(Rect selectionRect, GameObject gameObject)
    {
        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        //ボタン位置を調整場合は、xMaxへのマイナス値を変更する
        Rect btnRect = new Rect(selectionRect.xMax - 52, selectionRect.y, 28, selectionRect.height);
        string newTags;

        if (isEditorOnly)
        {
            btnStyle.normal.textColor = new Color(1.0f, 0.4f, 0.4f, 1.0f);
            btnStyle.hover.textColor = new Color(1.0f, 0.4f, 0.4f, 1.0f);
            newTags = "Untagged";
        }
        else
        {
            btnStyle.normal.textColor = Color.white;
            btnStyle.hover.textColor = Color.white;
            newTags = "EditorOnly";
        }

        if (GUI.Button(btnRect, "EO", btnStyle))
        {
            // ボタンがクリックされた時の処理
            gameObject.tag = newTags;
        }
    }

    /// <summary>
    /// ゲームオブジェクトのアクティブ/非アクティブトグルを処理します。
    /// </summary>
    private static void HandleActiveToggle(Rect selectionRect, GameObject gameObject)
    {
        //ボタン位置を調整場合は、xMaxへのマイナス値を変更する
        Rect toggleRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 20, selectionRect.height);

        bool active = GUI.Toggle(toggleRect, gameObject.activeSelf, string.Empty);
        if (active == gameObject.activeSelf)
        {
            return;
        }

        // アクティブトグルが変更された時の処理
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 背景スタイルを取得します。
    /// </summary>
    private static GUIStyle GetBackgroundStyle()
    {
        GUIStyle backgroundStyle = new GUIStyle(GUI.skin.label);
        //再生モード終了時にbackgroundTextureがnullになってしまうため、
        //backgroundTextureをセットするのではなく、GetBackgroundTextureを再実行する
        backgroundStyle.normal.background = GetBackgroundTexture(defBackColor);
        return backgroundStyle;
    }

    /// <summary>
    /// 背景テクスチャを取得するメソッド。既存のテクスチャを再利用または生成します。
    /// </summary>
    private static Texture2D GetBackgroundTexture(Color color)
    {
        if (backgroundTexture == null)
        {
            backgroundTexture = MakeTex(2, 2, color);
        }
        else
        {
            UpdateTextureColor(backgroundTexture, color);
        }
        return backgroundTexture;
    }

    /// <summary>
    /// テクスチャの色を更新します。
    /// </summary>
    private static void UpdateTextureColor(Texture2D texture, Color color)
    {
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    /// <summary>
    /// 新しい Texture2D を生成します。
    /// </summary>
    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}