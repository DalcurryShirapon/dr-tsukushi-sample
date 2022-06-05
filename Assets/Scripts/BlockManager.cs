using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    const int HORIZONTAL_NUM = 8;  // 横列数
    const int VERTICAL_NUM = 15;   // 縦列数

    [SerializeField]
    GameObject blockPrefab;

    [SerializeField]
    Player player;

    // ブロック配置情報
    Block[,] blockInfo = new Block[HORIZONTAL_NUM, VERTICAL_NUM];

    bool isErase;  // ブロックの消去があるか
    bool isFall;   // ブロックの降下があるか

    // 瓶の中のウィルスリスト（ゲームクリア判定用）
    List<Block> virusBlockList = new List<Block>();

    private void Start()
    {
        StageStart();
    }

    /// <summary>
    /// ステージ開始
    /// </summary>
    private void StageStart()
    {
        player.Init(PlayerTurnEndCallback, GetBlockInfo, IsOutBottle);

        CreateStage();

        CreatePlayerBlock();
    }

    /// <summary>
    /// ステージ生成
    /// </summary>
    private void CreateStage()
    {
        virusBlockList.Clear();

        // ウィルスブロックを生成しつつリストに追加
        virusBlockList.Add( CreateBlock(Block.BlockType.VirusRed, 2, 8) );
        virusBlockList.Add( CreateBlock(Block.BlockType.VirusBlue, 5, 10) );
        virusBlockList.Add( CreateBlock(Block.BlockType.VirusYellow, 7, 12) );
    }

    /// <summary>
    /// ブロック生成
    /// </summary>
    /// <param name="type">ブロックの種類</param>
    /// <param name="x">横配置位置</param>
    /// <param name="y">縦配置位置</param>
    private Block CreateBlock(Block.BlockType type, int x, int y)
    {
        // プレハブから生成
        GameObject block = GameObject.Instantiate(blockPrefab);
        block.transform.parent = transform;

        // ブロックスクリプトで初期セッティング
        Block blockCom = block.GetComponent<Block>();
        blockCom.Setting(type, Block.BlockRota.None, x, y, GetBlockInfo, IsOutBottle);

        // 配置情報に追加
        blockInfo[x, y] = blockCom;

        return blockCom;
    }

    /// <summary>
    /// プレイヤー用ブロック生成
    /// </summary>
    private void CreatePlayerBlock()
    {
        // 生成先にブロックが既に存在するなら
        if (GetBlockInfo(3, 1) != null || GetBlockInfo(4, 1) != null)
        {
            // ゲームオーバー
            Debug.Log("Game Over");
            return;
        }

        // 左ブロック生成
        GameObject blockL = GameObject.Instantiate(blockPrefab);
        blockL.transform.parent = transform;

        Block blockLCom = blockL.GetComponent<Block>();
        Block.BlockType blockLType = GetRandomCap();

        blockLCom.Setting(blockLType, Block.BlockRota.Left, 3, 1, GetBlockInfo, IsOutBottle);

        // 右ブロック生成
        GameObject blockR = GameObject.Instantiate(blockPrefab);
        blockR.transform.parent = transform;

        Block blockRCom = blockR.GetComponent<Block>();
        Block.BlockType blockRType = GetRandomCap();

        blockRCom.Setting(blockRType, Block.BlockRota.Right, 4, 1, GetBlockInfo, IsOutBottle);

        // プレイヤーが操作できるようにブロックコンポーネントをプレイヤークラスに渡す
        player.SetPlayerBlock(new Block[] { blockLCom, blockRCom });
    }

    /// <summary>
    /// カプセルをランダムに選ぶ
    /// </summary>
    /// <returns></returns>
    private Block.BlockType GetRandomCap()
    {
        Block.BlockType type = Block.BlockType.CapRed;

        int id = UnityEngine.Random.Range(0, 3);

        switch (id)
        {
            case 0: type = Block.BlockType.CapRed; break;
            case 1: type = Block.BlockType.CapBlue; break;
            case 2: type = Block.BlockType.CapYellow; break;
        }

        return type;
    }

    /// <summary>
    /// カプセルを配置してプレイヤー操作が終わった際のコールバック処理
    /// </summary>
    /// <param name="playerBlock"></param>
    private void PlayerTurnEndCallback(Block[] playerBlock)
    {
        // プレイヤーの操作していたブロックの情報を配置情報に渡す
        int leftX = playerBlock[0].BlockPos[Block.BLOCK_POS_X_IDX];
        int leftY = playerBlock[0].BlockPos[Block.BLOCK_POS_Y_IDX];

        blockInfo[leftX, leftY] = playerBlock[0];

        int rightX = playerBlock[1].BlockPos[Block.BLOCK_POS_X_IDX];
        int rightY = playerBlock[1].BlockPos[Block.BLOCK_POS_Y_IDX];

        blockInfo[rightX, rightY] = playerBlock[1];

        // ブロックの消去、降下の実行
        StartCoroutine(BlockEraseProcess());
    }

    /// <summary>
    /// ブロック消去プロセス
    /// </summary>
    /// <returns></returns>
    private IEnumerator BlockEraseProcess()
    {
        isErase = false;

        // ブロック消去チェック
        CheckEraseBlock();

        if (isErase)
        {
            // ブロック消去演出
            yield return StartCoroutine(PlayEraseBlock());

            if (IsGameClear())
            {
                // ゲームクリア
                Debug.Log("Game Clear");
                yield break;
            }

            // 落下ブロックがある限り落下処理を続ける
            do
            {
                isFall = false;

                // ブロック落下チェック
                CheckFallBlock();

                // ブロック落下演出
                if (isFall)
                {
                    yield return StartCoroutine(PlayFallBlock());
                }
            }
            while (isFall);

            // 消去するブロックがなくなるまで続ける
            StartCoroutine(BlockEraseProcess());
        }
        else
        {
            // 少し間を開ける
            float time = 0;
            while (time < 0.25f)
            {
                time += Time.deltaTime;
                yield return null;
            }

            // プレイヤーブロックを生成
            CreatePlayerBlock();
        }
    }

    /// <summary>
    /// ブロック消去チェック
    /// </summary>
    private void CheckEraseBlock()
    {
        // 配置できる場所をすべてチェック
        for (int h = 0; h < HORIZONTAL_NUM; h++)
        {
            for (int v = VERTICAL_NUM - 1; v >= 0; v--)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null) continue;

                CheckHorizontal(b, h, v);
                CheckVertical(b, h, v);
            }
        }
    }

    /// <summary>
    /// 横列に揃っているかチェック
    /// </summary>
    /// <param name="b">チェック対象のブロック</param>
    /// <param name="h">横位置</param>
    /// <param name="v">縦位置</param>
    private void CheckHorizontal(Block b, int h, int v)
    {
        List<Block> sameBlockList = new List<Block>();

        // 同色のブロックが続いていたらリストに格納
        for (int i = h + 1; i < HORIZONTAL_NUM; i++)
        {
            Block t = GetBlockInfo(i, v);

            if (t == null) break;

            if (IsSameColorBlock(b.CurrentBlockType, t.CurrentBlockType))
            {
                sameBlockList.Add(t);
            }
            else
            {
                break;
            }
        }

        // チェック対象含めて4つ以上なら消去フラグを立てる
        if (sameBlockList.Count >= 3)
        {
            b.IsErase = true;
            foreach (var s in sameBlockList)
            {
                s.IsErase = true;
            }

            isErase = true;
        }
    }

    /// <summary>
    /// 縦列に揃っているかチェック
    /// </summary>
    /// <param name="b">チェック対象のブロック</param>
    /// <param name="h">横位置</param>
    /// <param name="v">縦位置</param>
    private void CheckVertical(Block b, int h, int v)
    {
        List<Block> sameBlockList = new List<Block>();

        // 同色のブロックが続いていたらリストに格納
        for (int i = v - 1; i >= 0; i--)
        {
            Block t = GetBlockInfo(h, i);

            if (t == null) break;

            if (IsSameColorBlock(b.CurrentBlockType, t.CurrentBlockType))
            {
                sameBlockList.Add(t);
            }
            else
            {
                break;
            }
        }

        // チェック対象含めて4つ以上なら消去フラグを立てる
        if (sameBlockList.Count >= 3)
        {
            b.IsErase = true;
            foreach (var s in sameBlockList)
            {
                s.IsErase = true;
            }

            isErase = true;
        }
    }

    /// <summary>
    /// 同色ブロックか判定
    /// </summary>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool IsSameColorBlock(Block.BlockType b, Block.BlockType t)
    {
        switch (b)
        {
            case Block.BlockType.CapRed:
            case Block.BlockType.SingleCapRed:
            case Block.BlockType.VirusRed:
                return (t == Block.BlockType.CapRed || t == Block.BlockType.SingleCapRed || t == Block.BlockType.VirusRed);

            case Block.BlockType.CapBlue:
            case Block.BlockType.SingleCapBlue:
            case Block.BlockType.VirusBlue:
                return (t == Block.BlockType.CapBlue || t == Block.BlockType.SingleCapBlue || t == Block.BlockType.VirusBlue);

            case Block.BlockType.CapYellow:
            case Block.BlockType.SingleCapYellow:
            case Block.BlockType.VirusYellow:
                return (t == Block.BlockType.CapYellow || t == Block.BlockType.SingleCapYellow || t == Block.BlockType.VirusYellow);

        }

        return false;
    }

    /// <summary>
    /// ブロック消去演出
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayEraseBlock()
    {
        // ブロック消去実行
        for (int h = 0; h < HORIZONTAL_NUM; h++)
        {
            for (int v = VERTICAL_NUM - 1; v >= 0; v--)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null) continue;

                if (b.IsErase)
                {
                    // ウィルスリストに対象がいればリストから消去
                    if (virusBlockList.Contains(b))
                    {
                        virusBlockList.Remove(b);
                    }

                    // ブロック消去
                    Destroy(b.gameObject);
                    blockInfo[h, v] = null;
                }
            }
        }

        // カプセル変化
        for (int h = 0; h < HORIZONTAL_NUM; h++)
        {
            for (int v = VERTICAL_NUM - 1; v >= 0; v--)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null) continue;

                // カプセルの相方ブロックを取得
                Block bs = null;
                switch (b.CurrentBlockRota)
                {
                    case Block.BlockRota.Up:
                        bs = GetBlockInfo(h, v + 1);
                        break;

                    case Block.BlockRota.Left:
                        bs = GetBlockInfo(h + 1, v);
                        break;

                    case Block.BlockRota.Down:
                        bs = GetBlockInfo(h, v - 1);
                        break;

                    case Block.BlockRota.Right:
                        bs = GetBlockInfo(h - 1, v);
                        break;

                    default:
                        continue;
                }

                // 相方が消えていれば変形
                if (bs == null)
                {
                    b.ChangeCap();
                }
            }
        }

        // 少し間を開ける
        float time = 0;
        while (time < 0.25f)
        {
            time += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// ブロック落下チェック
    /// </summary>
    private void CheckFallBlock()
    {
        // 最下段は落下先がないので2段目からチェック
        for (int v = VERTICAL_NUM - 2; v >= 0; v--)
        {
            for (int h = 0; h < HORIZONTAL_NUM; h++)
            {
                Block b = GetBlockInfo(h, v);

                // ウィルスは落下しない
                if (b == null ||
                    b.CurrentBlockType == Block.BlockType.VirusRed ||
                    b.CurrentBlockType == Block.BlockType.VirusBlue ||
                    b.CurrentBlockType == Block.BlockType.VirusYellow)
                    continue;

                // 一つ下のブロック
                Block db = GetBlockInfo(h, v + 1);

                // 右向きカプセルなら以下のチェックを行う
                if (b.CurrentBlockRota == Block.BlockRota.Right)
                {
                    // 一つ左のブロック（相方カプセルのはず）
                    Block lb = GetBlockInfo(h - 1, v);

                    if (lb.IsFall && (db == null || db.IsFall))
                    {
                        // 相方が落ちるなら、さらに落下先があるなら落下させる
                        b.IsFall = true;
                        isFall = true;
                    }
                    else
                    {
                        // 自分も相方も落下させない
                        b.IsFall = false;
                        lb.IsFall = false;
                    }
                }
                else
                {
                    if (db == null || db.IsFall)
                    {
                        // 落下先があるなら落下させる
                        b.IsFall = true;
                        // 左向きカプセルなら相方（右向き）が落ちるか分かるまで落下処理をオンにしない
                        if (b.CurrentBlockRota != Block.BlockRota.Left)
                        {
                            isFall = true;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// ブロック落下演出
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayFallBlock()
    {
        // 落下移動
        for (int v = VERTICAL_NUM - 2; v >= 0; v--)
        {
            for (int h = 0; h < HORIZONTAL_NUM; h++)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null || !b.IsFall) continue;

                // 落下処理
                b.IsFall = false;
                b.Move(Block.BlockMove.Down);

                // ブロック情報を下に移行
                blockInfo[h, v] = null;
                blockInfo[h, v + 1] = b;
            }
        }

        // 少し間を開ける
        float time = 0;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 配置されたブロック情報を取得
    /// </summary>
    /// <param name="x">X位置</param>
    /// <param name="y">Y位置</param>
    /// <returns></returns>
    private Block GetBlockInfo(int x, int y)
    {
        if (IsOutBottle(x, y))
        {
            return null;
        }

        return blockInfo[x, y];
    }

    /// <summary>
    /// 瓶の外か判定
    /// </summary>
    /// <param name="x">X位置</param>
    /// <param name="y">Y位置</param>
    /// <returns></returns>
    private bool IsOutBottle(int x, int y)
    {
        return (x < 0 || x >= HORIZONTAL_NUM || y < 0 || y >= VERTICAL_NUM);
    }

    /// <summary>
    /// ゲームクリア状態か判定
    /// </summary>
    /// <returns></returns>
    private bool IsGameClear()
    {
        return virusBlockList.Count == 0;
    }
}
