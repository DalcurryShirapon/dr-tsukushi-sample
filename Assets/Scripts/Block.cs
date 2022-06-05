using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    const float LEFT_LIMIT_POS = -28f;
    const float UPPER_LIMIT_POS = 44f;
    const float BLOCK_DISTANCE = 8f;

    /// <summary>
    /// ブロックの種類
    /// </summary>
    public enum BlockType
    {
        CapRed,
        CapBlue,
        CapYellow,
        SingleCapRed,
        SingleCapBlue,
        SingleCapYellow,
        VirusRed,
        VirusBlue,
        VirusYellow,
    }

    /// <summary>
    /// ブロックの回転状況
    /// </summary>
    public enum BlockRota
    {
        None,
        Up,
        Left,
        Down,
        Right,
    }

    /// <summary>
    /// ブロックの移動
    /// </summary>
    public enum BlockMove
    {
        Up,
        Left,
        Down,
        Right,
    }

    [SerializeField]
    SpriteRenderer renderer;
    [SerializeField]
    Sprite[] sprites;

    Func<int, int, Block> GetBlockInfo;  // ブロック情報取得用
    Func<int, int, bool> IsOutBottle;    // 瓶の外か判定用

    /// <summary>
    /// ブロック位置（0 = X位置, 1 = Y位置）
    /// </summary>
    public int[] BlockPos
    {
        get; set;
    }

    public const int BLOCK_POS_X_IDX = 0;
    public const int BLOCK_POS_Y_IDX = 1;

    /// <summary>
    /// 現在のブロック種類
    /// </summary>
    public BlockType CurrentBlockType
    {
        get; private set;
    }

    /// <summary>
    /// 現在のブロック回転状況
    /// </summary>
    public BlockRota CurrentBlockRota
    {
        get; private set;
    }

    /// <summary>
    /// ブロックの消去を行うか
    /// </summary>
    public bool IsErase
    {
        get; set;
    }

    /// <summary>
    /// ブロックの降下を行うか
    /// </summary>
    public bool IsFall
    {
        get; set;
    }

    /// <summary>
    /// 初期セッティング
    /// </summary>
    /// <param name="type">ブロックの種類</param>
    /// <param name="rota">ブロックの回転状況</param>
    /// <param name="x">配置するX位置</param>
    /// <param name="y">配置するY位置</param>
    /// <param name="getBlockInfo">ブロック情報取得用</param>
    /// <param name="isOutBottle">瓶の外か判定用</param>
    public void Setting(BlockType type, BlockRota rota, int x, int y, Func<int, int, Block> getBlockInfo, Func<int, int, bool> isOutBottle)
    {
        float bx = LEFT_LIMIT_POS + BLOCK_DISTANCE * x;
        float by = UPPER_LIMIT_POS - BLOCK_DISTANCE * y;
        transform.localPosition = new Vector3(bx, by);

        BlockPos = new int[] { x, y };

        ChangeBlock(type);

        Rotate(rota);

        GetBlockInfo = getBlockInfo;
        IsOutBottle = isOutBottle;
    }

    /// <summary>
    /// ブロック変化
    /// </summary>
    /// <param name="type">ブロックの種類</param>
    public void ChangeBlock(BlockType type)
    {
        CurrentBlockType = type;

        switch (CurrentBlockType)
        {
            case BlockType.CapRed:
                renderer.sprite = sprites[(int)BlockType.CapRed];
                break;

            case BlockType.CapBlue:
                renderer.sprite = sprites[(int)BlockType.CapBlue];
                break;

            case BlockType.CapYellow:
                renderer.sprite = sprites[(int)BlockType.CapYellow];
                break;

            case BlockType.SingleCapRed:
                renderer.sprite = sprites[(int)BlockType.SingleCapRed];
                break;

            case BlockType.SingleCapBlue:
                renderer.sprite = sprites[(int)BlockType.SingleCapBlue];
                break;

            case BlockType.SingleCapYellow:
                renderer.sprite = sprites[(int)BlockType.SingleCapYellow];
                break;

            case BlockType.VirusRed:
                renderer.sprite = sprites[(int)BlockType.VirusRed];
                break;

            case BlockType.VirusBlue:
                renderer.sprite = sprites[(int)BlockType.VirusBlue];
                break;

            case BlockType.VirusYellow:
                renderer.sprite = sprites[(int)BlockType.VirusYellow];
                break;
        }
    }

    /// <summary>
    /// ブロック回転
    /// </summary>
    public void Rotate()
    {
        switch (CurrentBlockRota)
        {
            case BlockRota.Up:
                CurrentBlockRota = BlockRota.Left;
                break;

            case BlockRota.Left:
                CurrentBlockRota = BlockRota.Down;
                break;

            case BlockRota.Down:
                CurrentBlockRota = BlockRota.Right;
                break;

            case BlockRota.Right:
                CurrentBlockRota = BlockRota.Up;
                break;
        }

        Rotate(CurrentBlockRota);
    }

    /// <summary>
    /// ブロック回転
    /// </summary>
    /// <param name="rota">回転先</param>
    private void Rotate(BlockRota rota)
    {
        CurrentBlockRota = rota;

        switch (CurrentBlockRota)
        {
            case BlockRota.Up:
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;

            case BlockRota.Left:
                transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;

            case BlockRota.Down:
                transform.localRotation = Quaternion.Euler(0, 180, 180);
                break;

            case BlockRota.Right:
                transform.localRotation = Quaternion.Euler(0, 180, 90);
                break;
        }
    }

    /// <summary>
    /// ブロック移動
    /// </summary>
    /// <param name="move">移動先</param>
    /// <param name="conflictCB">衝突時のコールバック</param>
    /// <param name="successCB">移動成功時のコールバック</param>
    public void Move(BlockMove move, Action<BlockMove> conflictCB = null, Action<BlockMove> successCB = null)
    {
        // 移動処理
        switch (move)
        {
            case BlockMove.Up:
                transform.localPosition += new Vector3(0, BLOCK_DISTANCE, 0);
                BlockPos[BLOCK_POS_Y_IDX] -= 1;
                break;

            case BlockMove.Left:
                transform.localPosition += new Vector3(-BLOCK_DISTANCE, 0, 0);
                BlockPos[BLOCK_POS_X_IDX] -= 1;
                break;

            case BlockMove.Down:
                transform.localPosition += new Vector3(0, -BLOCK_DISTANCE, 0);
                BlockPos[BLOCK_POS_Y_IDX] += 1;
                break;

            case BlockMove.Right:
                transform.localPosition += new Vector3(BLOCK_DISTANCE, 0, 0);
                BlockPos[BLOCK_POS_X_IDX] += 1;
                break;
        }

        // 移動先が瓶の外なら衝突
        if (IsOutBottle(BlockPos[BLOCK_POS_X_IDX], BlockPos[BLOCK_POS_Y_IDX]))
        {
            if (conflictCB != null)
            {
                conflictCB(move);
                return;
            }
        }

        // 移動先にブロックがあるなら衝突
        Block moveToBlock = GetBlockInfo(BlockPos[BLOCK_POS_X_IDX], BlockPos[BLOCK_POS_Y_IDX]);
        if (moveToBlock != null)
        {
            if (conflictCB != null)
            {
                conflictCB(move);
                return;
            }
        }

        // 移動成功
        if (successCB != null)
        {
            successCB(move);
        }
    }

    /// <summary>
    /// カプセル変化（連結していない単体にする）
    /// </summary>
    public void ChangeCap()
    {
        // 変化処理
        switch (CurrentBlockType)
        {
            case BlockType.CapRed:
                renderer.sprite = sprites[(int)BlockType.SingleCapRed];
                CurrentBlockType = BlockType.SingleCapRed;
                break;

            case BlockType.CapBlue:
                renderer.sprite = sprites[(int)BlockType.SingleCapBlue];
                CurrentBlockType = BlockType.SingleCapBlue;
                break;

            case BlockType.CapYellow:
                renderer.sprite = sprites[(int)BlockType.SingleCapYellow];
                CurrentBlockType = BlockType.SingleCapYellow;
                break;
        }

        CurrentBlockRota = BlockRota.None;
        renderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    /// <summary>
    /// 移動先の反対方向を取得
    /// </summary>
    /// <param name="move">移動先</param>
    /// <returns></returns>
    public static BlockMove ReverseMove(BlockMove move)
    {
        switch (move)
        {
            case BlockMove.Up:
                return BlockMove.Down;

            case BlockMove.Left:
                return BlockMove.Right;

            case BlockMove.Down:
                return BlockMove.Up;

            case BlockMove.Right:
                return BlockMove.Left;
        }

        return BlockMove.Up;
    }
}
