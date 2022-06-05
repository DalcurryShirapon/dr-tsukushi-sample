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
    /// �u���b�N�̎��
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
    /// �u���b�N�̉�]��
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
    /// �u���b�N�̈ړ�
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

    Func<int, int, Block> GetBlockInfo;  // �u���b�N���擾�p
    Func<int, int, bool> IsOutBottle;    // �r�̊O������p

    /// <summary>
    /// �u���b�N�ʒu�i0 = X�ʒu, 1 = Y�ʒu�j
    /// </summary>
    public int[] BlockPos
    {
        get; set;
    }

    public const int BLOCK_POS_X_IDX = 0;
    public const int BLOCK_POS_Y_IDX = 1;

    /// <summary>
    /// ���݂̃u���b�N���
    /// </summary>
    public BlockType CurrentBlockType
    {
        get; private set;
    }

    /// <summary>
    /// ���݂̃u���b�N��]��
    /// </summary>
    public BlockRota CurrentBlockRota
    {
        get; private set;
    }

    /// <summary>
    /// �u���b�N�̏������s����
    /// </summary>
    public bool IsErase
    {
        get; set;
    }

    /// <summary>
    /// �u���b�N�̍~�����s����
    /// </summary>
    public bool IsFall
    {
        get; set;
    }

    /// <summary>
    /// �����Z�b�e�B���O
    /// </summary>
    /// <param name="type">�u���b�N�̎��</param>
    /// <param name="rota">�u���b�N�̉�]��</param>
    /// <param name="x">�z�u����X�ʒu</param>
    /// <param name="y">�z�u����Y�ʒu</param>
    /// <param name="getBlockInfo">�u���b�N���擾�p</param>
    /// <param name="isOutBottle">�r�̊O������p</param>
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
    /// �u���b�N�ω�
    /// </summary>
    /// <param name="type">�u���b�N�̎��</param>
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
    /// �u���b�N��]
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
    /// �u���b�N��]
    /// </summary>
    /// <param name="rota">��]��</param>
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
    /// �u���b�N�ړ�
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="conflictCB">�Փˎ��̃R�[���o�b�N</param>
    /// <param name="successCB">�ړ��������̃R�[���o�b�N</param>
    public void Move(BlockMove move, Action<BlockMove> conflictCB = null, Action<BlockMove> successCB = null)
    {
        // �ړ�����
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

        // �ړ��悪�r�̊O�Ȃ�Փ�
        if (IsOutBottle(BlockPos[BLOCK_POS_X_IDX], BlockPos[BLOCK_POS_Y_IDX]))
        {
            if (conflictCB != null)
            {
                conflictCB(move);
                return;
            }
        }

        // �ړ���Ƀu���b�N������Ȃ�Փ�
        Block moveToBlock = GetBlockInfo(BlockPos[BLOCK_POS_X_IDX], BlockPos[BLOCK_POS_Y_IDX]);
        if (moveToBlock != null)
        {
            if (conflictCB != null)
            {
                conflictCB(move);
                return;
            }
        }

        // �ړ�����
        if (successCB != null)
        {
            successCB(move);
        }
    }

    /// <summary>
    /// �J�v�Z���ω��i�A�����Ă��Ȃ��P�̂ɂ���j
    /// </summary>
    public void ChangeCap()
    {
        // �ω�����
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
    /// �ړ���̔��Ε������擾
    /// </summary>
    /// <param name="move">�ړ���</param>
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
