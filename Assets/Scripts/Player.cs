using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    const float INPUT_INTERVAL = 0.2f;  // ���͊Ԋu����
    const float FALL_INTERVAL = 0.7f;   // �~���Ԋu����

    Block[] playerBlock = new Block[2]; // ���삷��u���b�N
    float inputMoveInterval = 0;        // �ړ�������͊Ԋu�J�E���^�[
    float inputRotaInterval = 0;        // ��]������͊Ԋu�J�E���^�[
    float fallInterval = 0;             // �~���Ԋu�J�E���^�[
    bool conflicted = false;            // �ړ����ɏՓ˂���
    int moveIdx;                        // ���݂̈ړ������z��ԍ�

    Action<Block[]> PlayerTurnEndCB;    // �v���C���[���슮���R�[���o�b�N
    Func<int, int, Block> GetBlockInfo; // �u���b�N�z�u���擾
    Func<int, int, bool> IsOutBottle;   // �r�̊O������

    /// <summary>
    /// ������
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
    /// �v���C���[����u���b�N���Z�b�g
    /// </summary>
    /// <param name="blocks"></param>
    public void SetPlayerBlock(Block[] blocks)
    {
        playerBlock[0] = blocks[0]; // ��
        playerBlock[1] = blocks[1]; // �E

        fallInterval = FALL_INTERVAL;
    }

    private void Update()
    {
        PlayerControll();
    }

    /// <summary>
    /// �v���C���[���쏈��
    /// </summary>
    private void PlayerControll()
    {
        // �u���b�N���Ȃ���Α��삳���Ȃ�
        if (playerBlock[0] == null || playerBlock[1] == null)
        {
            return;
        }

        PlayerBlockRotaControll();
        PlayerBlockMoveControll();
        
        FallBlock();
    }

    /// <summary>
    /// �u���b�N�ړ����͌��m
    /// </summary>
    private void PlayerBlockMoveControll()
    {
        // �C���^�[�o�����͑��삳���Ȃ�
        if (inputMoveInterval > 0)
        {
            inputMoveInterval -= Time.deltaTime;
            return;
        }

        // �v���C���[�̈ړ����͌��m
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x != 0)
        {
            // ������

            if (x > 0)
            {
                // �E����
                PlayerBlockMove(Block.BlockMove.Right);
            }
            else
            {
                // ������
                PlayerBlockMove(Block.BlockMove.Left);
            }

            inputMoveInterval = INPUT_INTERVAL;
        }
        else if (y != 0)
        {
            // �c����

            if (y < 0)
            {
                // ������
                PlayerBlockMove(Block.BlockMove.Down);
                inputMoveInterval = INPUT_INTERVAL / 2f;
                fallInterval = FALL_INTERVAL;
            }
        }
    }

    /// <summary>
    /// �u���b�N��]���͌��m
    /// </summary>
    private void PlayerBlockRotaControll()
    {
        // �C���^�[�o�����͑��삳���Ȃ�
        if (inputRotaInterval > 0)
        {
            inputRotaInterval -= Time.deltaTime;
            return;
        }

        // �v���C���[�̉�]���͌��m
        if (Input.GetButtonDown("Jump"))
        {
            // ��]����
            PlayerBlockRotate();
            inputRotaInterval = INPUT_INTERVAL;
        }
    }

    /// <summary>
    /// �����~��
    /// </summary>
    private void FallBlock()
    {
        // �~���C���^�[�o��
        if (fallInterval > 0)
        {
            fallInterval -= Time.deltaTime;
            return;
        }

        // �����~������
        fallInterval = FALL_INTERVAL;
        PlayerBlockMove(Block.BlockMove.Down);
    }

    /// <summary>
    /// �u���b�N�ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
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

            // ���̑���u���b�N�����łɏՓ˂��Ă���Ȃ�ȉ��̏��������Ȃ�
            if (conflicted)
            {
                break;
            }

            // �u���b�N�ړ�
            playerBlock[i].Move(move, ConflictCallback, SuccessMoveCallback);
        }
    }

    /// <summary>
    /// �ړ��Փˎ��R�[���o�b�N
    /// </summary>
    /// <param name="move">�ړ���</param>
    private void ConflictCallback(Block.BlockMove move)
    {
        if (move == Block.BlockMove.Down)
        {
            // ���ړ�

            // �u���b�N�����̈ʒu�ɖ߂�
            for (int i = 0; i < moveIdx; i++)
            {
                playerBlock[i].Move(Block.ReverseMove(move));
            }

            // �v���C���[���슮��
            PlayerTurnEndCB(playerBlock);
            playerBlock[0] = null;
            playerBlock[1] = null;
        }
        else
        {
            // ���ړ�

            // �u���b�N�����̈ʒu�ɖ߂�
            for (int i = 0; i < moveIdx; i++)
            {
                playerBlock[i].Move(Block.ReverseMove(move));
            }
        }

        // �Փ˂��܂���
        conflicted = true;
    }

    /// <summary>
    /// �ړ��������R�[���o�b�N
    /// </summary>
    /// <param name="move"></param>
    private void SuccessMoveCallback(Block.BlockMove move)
    {
        moveIdx++;

        // �ړ������o���Ƃ�
    }

    /// <summary>
    /// �u���b�N��]����
    /// </summary>
    private void PlayerBlockRotate()
    {
        if (playerBlock[0].CurrentBlockRota == Block.BlockRota.Up || playerBlock[0].CurrentBlockRota == Block.BlockRota.Down)
        {
            // �J�v�Z���̔z�u���c�̏ꍇ ��
            //                          ��

            // ���i�ɂ���J�v�Z���̃C���f�b�N�X
            int fidx = (playerBlock[0].BlockPos[Block.BLOCK_POS_Y_IDX] > playerBlock[1].BlockPos[Block.BLOCK_POS_Y_IDX]) ? 0 : 1;
            // ��i�ɂ���J�v�Z���̃C���f�b�N�X
            int sidx = (fidx == 0) ? 1 : 0;

            int px = playerBlock[fidx].BlockPos[Block.BLOCK_POS_X_IDX];
            int py = playerBlock[fidx].BlockPos[Block.BLOCK_POS_Y_IDX];

            if (GetBlockInfo(px + 1, py) == null && !IsOutBottle(px + 1, py))
            {
                // �J�v�Z�����i�́u�E�v�ɓ����悪����Ȃ�

                // ���i���E�Ɉړ�
                playerBlock[fidx].Move(Block.BlockMove.Right);
                playerBlock[fidx].Rotate();
                // ��i�����Ɉړ�
                playerBlock[sidx].Move(Block.BlockMove.Down);
                playerBlock[sidx].Rotate();
            }
            else if (GetBlockInfo(px - 1, py) == null && !IsOutBottle(px - 1, py))
            {
                // �J�v�Z�����i�́u���v�ɓ����悪����Ȃ�

                // ���i�͉摜��]�̂�
                playerBlock[fidx].Rotate();
                // ��i�͍����Ɉړ�
                playerBlock[sidx].Move(Block.BlockMove.Down);
                playerBlock[sidx].Move(Block.BlockMove.Left);
                playerBlock[sidx].Rotate();
            }
        }
        else
        {
            // �J�v�Z���̔z�u�����̏ꍇ ����

            // �E�ɂ���J�v�Z���̃C���f�b�N�X
            int fidx = (playerBlock[0].BlockPos[Block.BLOCK_POS_X_IDX] > playerBlock[1].BlockPos[Block.BLOCK_POS_X_IDX]) ? 0 : 1;
            // ���ɂ���J�v�Z���̃C���f�b�N�X
            int sidx = (fidx == 0) ? 1 : 0;

            int px = playerBlock[fidx].BlockPos[Block.BLOCK_POS_X_IDX];
            int py = playerBlock[fidx].BlockPos[Block.BLOCK_POS_Y_IDX];

            if (GetBlockInfo(px - 1, py - 1) == null && !IsOutBottle(px - 1, py - 1))
            {
                // �J�v�Z���E�́u����v�ɓ����悪����Ȃ�

                // �J�v�Z���E������Ɉړ�
                playerBlock[fidx].Move(Block.BlockMove.Left);
                playerBlock[fidx].Move(Block.BlockMove.Up);
                playerBlock[fidx].Rotate();
                // �J�v�Z�����͉摜��]�̂�
                playerBlock[sidx].Rotate();
            }
        }
    }
}
