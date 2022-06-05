using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    const float INPUT_INTERVAL = 0.2f;  // 入力間隔時間
    const float FALL_INTERVAL = 0.7f;   // 降下間隔時間

    Block[] playerBlock = new Block[2]; // 操作するブロック
    float inputMoveInterval = 0;        // 移動操作入力間隔カウンター
    float inputRotaInterval = 0;        // 回転操作入力間隔カウンター
    float fallInterval = 0;             // 降下間隔カウンター
    bool conflicted = false;            // 移動時に衝突した
    int moveIdx;                        // 現在の移動処理配列番号

    Action<Block[]> PlayerTurnEndCB;    // プレイヤー操作完了コールバック
    Func<int, int, Block> GetBlockInfo; // ブロック配置情報取得
    Func<int, int, bool> IsOutBottle;   // 瓶の外か判定

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="playerTurnEndCB"></param>
    /// <param name="getBlockInfo"></param>
    /// <param name="isOutBottle"></param>
    public void Init(Action<Block[]> playerTurnEndCB, Func<int, int, Block> getBlockInfo, Func<int, int, bool> isOutBottle)
    {
        PlayerTurnEndCB = playerTurnEndCB;
        GetBlockInfo = getBlockInfo;
        IsOutBottle = isOutBottle;
    }

    /// <summary>
    /// プレイヤー操作ブロックをセット
    /// </summary>
    /// <param name="blocks"></param>
    public void SetPlayerBlock(Block[] blocks)
    {
        playerBlock[0] = blocks[0]; // 左
        playerBlock[1] = blocks[1]; // 右

        fallInterval = FALL_INTERVAL;
    }

    private void Update()
    {
        PlayerControll();
    }

    /// <summary>
    /// プレイヤー操作処理
    /// </summary>
    private void PlayerControll()
    {
        // ブロックがなければ操作させない
        if (playerBlock[0] == null || playerBlock[1] == null)
        {
            return;
        }

        PlayerBlockRotaControll();
        PlayerBlockMoveControll();
        
        FallBlock();
    }

    /// <summary>
    /// ブロック移動入力検知
    /// </summary>
    private void PlayerBlockMoveControll()
    {
        // インターバル中は操作させない
        if (inputMoveInterval > 0)
        {
            inputMoveInterval -= Time.deltaTime;
            return;
        }

        // プレイヤーの移動入力検知
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x != 0)
        {
            // 横入力

            if (x > 0)
            {
                // 右入力
                PlayerBlockMove(Block.BlockMove.Right);
            }
            else
            {
                // 左入力
                PlayerBlockMove(Block.BlockMove.Left);
            }

            inputMoveInterval = INPUT_INTERVAL;
        }
        else if (y != 0)
        {
            // 縦入力

            if (y < 0)
            {
                // 下入力
                PlayerBlockMove(Block.BlockMove.Down);
                inputMoveInterval = INPUT_INTERVAL / 2f;
                fallInterval = FALL_INTERVAL;
            }
        }
    }

    /// <summary>
    /// ブロック回転入力検知
    /// </summary>
    private void PlayerBlockRotaControll()
    {
        // インターバル中は操作させない
        if (inputRotaInterval > 0)
        {
            inputRotaInterval -= Time.deltaTime;
            return;
        }

        // プレイヤーの回転入力検知
        if (Input.GetButtonDown("Jump"))
        {
            // 回転入力
            PlayerBlockRotate();
            inputRotaInterval = INPUT_INTERVAL;
        }
    }

    /// <summary>
    /// 自動降下
    /// </summary>
    private void FallBlock()
    {
        // 降下インターバル
        if (fallInterval > 0)
        {
            fallInterval -= Time.deltaTime;
            return;
        }

        // 自動降下処理
        fallInterval = FALL_INTERVAL;
        PlayerBlockMove(Block.BlockMove.Down);
    }

    /// <summary>
    /// ブロック移動処理
    /// </summary>
    /// <param name="move">移動先</param>
    private void PlayerBlockMove(Block.BlockMove move)
    {
        conflicted = false;
        moveIdx = 1;

        for (int i = 0; i < 2; i++)
        {
            if (playerBlock[i] == null)
            {
                break;
            }

            // 他の操作ブロックがすでに衝突しているなら以下の処理をしない
            if (conflicted)
            {
                break;
            }

            // ブロック移動
            playerBlock[i].Move(move, ConflictCallback, SuccessMoveCallback);
        }
    }

    /// <summary>
    /// 移動衝突時コールバック
    /// </summary>
    /// <param name="move">移動先</param>
    private void ConflictCallback(Block.BlockMove move)
    {
        if (move == Block.BlockMove.Down)
        {
            // 下移動

            // ブロックを元の位置に戻す
            for (int i = 0; i < moveIdx; i++)
            {
                playerBlock[i].Move(Block.ReverseMove(move));
            }

            // プレイヤー操作完了
            PlayerTurnEndCB(playerBlock);
            playerBlock[0] = null;
            playerBlock[1] = null;
        }
        else
        {
            // 横移動

            // ブロックを元の位置に戻す
            for (int i = 0; i < moveIdx; i++)
            {
                playerBlock[i].Move(Block.ReverseMove(move));
            }
        }

        // 衝突しました
        conflicted = true;
    }

    /// <summary>
    /// 移動成功時コールバック
    /// </summary>
    /// <param name="move"></param>
    private void SuccessMoveCallback(Block.BlockMove move)
    {
        moveIdx++;

        // 移動音を出すとか
    }

    /// <summary>
    /// ブロック回転処理
    /// </summary>
    private void PlayerBlockRotate()
    {
        if (playerBlock[0].CurrentBlockRota == Block.BlockRota.Up || playerBlock[0].CurrentBlockRota == Block.BlockRota.Down)
        {
            // カプセルの配置が縦の場合 ∩
            //                          ∪

            // 下段にあるカプセルのインデックス
            int fidx = (playerBlock[0].BlockPos[Block.BLOCK_POS_Y_IDX] > playerBlock[1].BlockPos[Block.BLOCK_POS_Y_IDX]) ? 0 : 1;
            // 上段にあるカプセルのインデックス
            int sidx = (fidx == 0) ? 1 : 0;

            int px = playerBlock[fidx].BlockPos[Block.BLOCK_POS_X_IDX];
            int py = playerBlock[fidx].BlockPos[Block.BLOCK_POS_Y_IDX];

            if (GetBlockInfo(px + 1, py) == null && !IsOutBottle(px + 1, py))
            {
                // カプセル下段の「右」に動く先があるなら

                // 下段を右に移動
                playerBlock[fidx].Move(Block.BlockMove.Right);
                playerBlock[fidx].Rotate();
                // 上段を下に移動
                playerBlock[sidx].Move(Block.BlockMove.Down);
                playerBlock[sidx].Rotate();
            }
            else if (GetBlockInfo(px - 1, py) == null && !IsOutBottle(px - 1, py))
            {
                // カプセル下段の「左」に動く先があるなら

                // 下段は画像回転のみ
                playerBlock[fidx].Rotate();
                // 上段は左下に移動
                playerBlock[sidx].Move(Block.BlockMove.Down);
                playerBlock[sidx].Move(Block.BlockMove.Left);
                playerBlock[sidx].Rotate();
            }
        }
        else
        {
            // カプセルの配置が横の場合 ⊂⊃

            // 右にあるカプセルのインデックス
            int fidx = (playerBlock[0].BlockPos[Block.BLOCK_POS_X_IDX] > playerBlock[1].BlockPos[Block.BLOCK_POS_X_IDX]) ? 0 : 1;
            // 左にあるカプセルのインデックス
            int sidx = (fidx == 0) ? 1 : 0;

            int px = playerBlock[fidx].BlockPos[Block.BLOCK_POS_X_IDX];
            int py = playerBlock[fidx].BlockPos[Block.BLOCK_POS_Y_IDX];

            if (GetBlockInfo(px - 1, py - 1) == null && !IsOutBottle(px - 1, py - 1))
            {
                // カプセル右の「左上」に動く先があるなら

                // カプセル右を左上に移動
                playerBlock[fidx].Move(Block.BlockMove.Left);
                playerBlock[fidx].Move(Block.BlockMove.Up);
                playerBlock[fidx].Rotate();
                // カプセル左は画像回転のみ
                playerBlock[sidx].Rotate();
            }
        }
    }
}
