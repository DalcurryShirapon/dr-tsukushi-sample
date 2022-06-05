using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    const int HORIZONTAL_NUM = 8;  // ����
    const int VERTICAL_NUM = 15;   // �c��

    [SerializeField]
    GameObject blockPrefab;

    [SerializeField]
    Player player;

    // �u���b�N�z�u���
    Block[,] blockInfo = new Block[HORIZONTAL_NUM, VERTICAL_NUM];

    bool isErase;  // �u���b�N�̏��������邩
    bool isFall;   // �u���b�N�̍~�������邩

    // �r�̒��̃E�B���X���X�g�i�Q�[���N���A����p�j
    List<Block> virusBlockList = new List<Block>();

    private void Start()
    {
        StageStart();
    }

    /// <summary>
    /// �X�e�[�W�J�n
    /// </summary>
    private void StageStart()
    {
        player.Init(PlayerTurnEndCallback, GetBlockInfo, IsOutBottle);

        CreateStage();

        CreatePlayerBlock();
    }

    /// <summary>
    /// �X�e�[�W����
    /// </summary>
    private void CreateStage()
    {
        virusBlockList.Clear();

        // �E�B���X�u���b�N�𐶐������X�g�ɒǉ�
        virusBlockList.Add( CreateBlock(Block.BlockType.VirusRed, 2, 8) );
        virusBlockList.Add( CreateBlock(Block.BlockType.VirusBlue, 5, 10) );
        virusBlockList.Add( CreateBlock(Block.BlockType.VirusYellow, 7, 12) );
    }

    /// <summary>
    /// �u���b�N����
    /// </summary>
    /// <param name="type">�u���b�N�̎��</param>
    /// <param name="x">���z�u�ʒu</param>
    /// <param name="y">�c�z�u�ʒu</param>
    private Block CreateBlock(Block.BlockType type, int x, int y)
    {
        // �v���n�u���琶��
        GameObject block = GameObject.Instantiate(blockPrefab);
        block.transform.parent = transform;

        // �u���b�N�X�N���v�g�ŏ����Z�b�e�B���O
        Block blockCom = block.GetComponent<Block>();
        blockCom.Setting(type, Block.BlockRota.None, x, y, GetBlockInfo, IsOutBottle);

        // �z�u���ɒǉ�
        blockInfo[x, y] = blockCom;

        return blockCom;
    }

    /// <summary>
    /// �v���C���[�p�u���b�N����
    /// </summary>
    private void CreatePlayerBlock()
    {
        // ������Ƀu���b�N�����ɑ��݂���Ȃ�
        if (GetBlockInfo(3, 1) != null || GetBlockInfo(4, 1) != null)
        {
            // �Q�[���I�[�o�[
            Debug.Log("Game Over");
            return;
        }

        // ���u���b�N����
        GameObject blockL = GameObject.Instantiate(blockPrefab);
        blockL.transform.parent = transform;

        Block blockLCom = blockL.GetComponent<Block>();
        Block.BlockType blockLType = GetRandomCap();

        blockLCom.Setting(blockLType, Block.BlockRota.Left, 3, 1, GetBlockInfo, IsOutBottle);

        // �E�u���b�N����
        GameObject blockR = GameObject.Instantiate(blockPrefab);
        blockR.transform.parent = transform;

        Block blockRCom = blockR.GetComponent<Block>();
        Block.BlockType blockRType = GetRandomCap();

        blockRCom.Setting(blockRType, Block.BlockRota.Right, 4, 1, GetBlockInfo, IsOutBottle);

        // �v���C���[������ł���悤�Ƀu���b�N�R���|�[�l���g���v���C���[�N���X�ɓn��
        player.SetPlayerBlock(new Block[] { blockLCom, blockRCom });
    }

    /// <summary>
    /// �J�v�Z���������_���ɑI��
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
    /// �J�v�Z����z�u���ăv���C���[���삪�I������ۂ̃R�[���o�b�N����
    /// </summary>
    /// <param name="playerBlock"></param>
    private void PlayerTurnEndCallback(Block[] playerBlock)
    {
        // �v���C���[�̑��삵�Ă����u���b�N�̏���z�u���ɓn��
        int leftX = playerBlock[0].BlockPos[Block.BLOCK_POS_X_IDX];
        int leftY = playerBlock[0].BlockPos[Block.BLOCK_POS_Y_IDX];

        blockInfo[leftX, leftY] = playerBlock[0];

        int rightX = playerBlock[1].BlockPos[Block.BLOCK_POS_X_IDX];
        int rightY = playerBlock[1].BlockPos[Block.BLOCK_POS_Y_IDX];

        blockInfo[rightX, rightY] = playerBlock[1];

        // �u���b�N�̏����A�~���̎��s
        StartCoroutine(BlockEraseProcess());
    }

    /// <summary>
    /// �u���b�N�����v���Z�X
    /// </summary>
    /// <returns></returns>
    private IEnumerator BlockEraseProcess()
    {
        isErase = false;

        // �u���b�N�����`�F�b�N
        CheckEraseBlock();

        if (isErase)
        {
            // �u���b�N�������o
            yield return StartCoroutine(PlayEraseBlock());

            if (IsGameClear())
            {
                // �Q�[���N���A
                Debug.Log("Game Clear");
                yield break;
            }

            // �����u���b�N��������藎�������𑱂���
            do
            {
                isFall = false;

                // �u���b�N�����`�F�b�N
                CheckFallBlock();

                // �u���b�N�������o
                if (isFall)
                {
                    yield return StartCoroutine(PlayFallBlock());
                }
            }
            while (isFall);

            // ��������u���b�N���Ȃ��Ȃ�܂ő�����
            StartCoroutine(BlockEraseProcess());
        }
        else
        {
            // �����Ԃ��J����
            float time = 0;
            while (time < 0.25f)
            {
                time += Time.deltaTime;
                yield return null;
            }

            // �v���C���[�u���b�N�𐶐�
            CreatePlayerBlock();
        }
    }

    /// <summary>
    /// �u���b�N�����`�F�b�N
    /// </summary>
    private void CheckEraseBlock()
    {
        // �z�u�ł���ꏊ�����ׂă`�F�b�N
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
    /// ����ɑ����Ă��邩�`�F�b�N
    /// </summary>
    /// <param name="b">�`�F�b�N�Ώۂ̃u���b�N</param>
    /// <param name="h">���ʒu</param>
    /// <param name="v">�c�ʒu</param>
    private void CheckHorizontal(Block b, int h, int v)
    {
        List<Block> sameBlockList = new List<Block>();

        // ���F�̃u���b�N�������Ă����烊�X�g�Ɋi�[
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

        // �`�F�b�N�Ώۊ܂߂�4�ȏ�Ȃ�����t���O�𗧂Ă�
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
    /// �c��ɑ����Ă��邩�`�F�b�N
    /// </summary>
    /// <param name="b">�`�F�b�N�Ώۂ̃u���b�N</param>
    /// <param name="h">���ʒu</param>
    /// <param name="v">�c�ʒu</param>
    private void CheckVertical(Block b, int h, int v)
    {
        List<Block> sameBlockList = new List<Block>();

        // ���F�̃u���b�N�������Ă����烊�X�g�Ɋi�[
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

        // �`�F�b�N�Ώۊ܂߂�4�ȏ�Ȃ�����t���O�𗧂Ă�
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
    /// ���F�u���b�N������
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
    /// �u���b�N�������o
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayEraseBlock()
    {
        // �u���b�N�������s
        for (int h = 0; h < HORIZONTAL_NUM; h++)
        {
            for (int v = VERTICAL_NUM - 1; v >= 0; v--)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null) continue;

                if (b.IsErase)
                {
                    // �E�B���X���X�g�ɑΏۂ�����΃��X�g�������
                    if (virusBlockList.Contains(b))
                    {
                        virusBlockList.Remove(b);
                    }

                    // �u���b�N����
                    Destroy(b.gameObject);
                    blockInfo[h, v] = null;
                }
            }
        }

        // �J�v�Z���ω�
        for (int h = 0; h < HORIZONTAL_NUM; h++)
        {
            for (int v = VERTICAL_NUM - 1; v >= 0; v--)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null) continue;

                // �J�v�Z���̑����u���b�N���擾
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

                // �����������Ă���Εό`
                if (bs == null)
                {
                    b.ChangeCap();
                }
            }
        }

        // �����Ԃ��J����
        float time = 0;
        while (time < 0.25f)
        {
            time += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// �u���b�N�����`�F�b�N
    /// </summary>
    private void CheckFallBlock()
    {
        // �ŉ��i�͗����悪�Ȃ��̂�2�i�ڂ���`�F�b�N
        for (int v = VERTICAL_NUM - 2; v >= 0; v--)
        {
            for (int h = 0; h < HORIZONTAL_NUM; h++)
            {
                Block b = GetBlockInfo(h, v);

                // �E�B���X�͗������Ȃ�
                if (b == null ||
                    b.CurrentBlockType == Block.BlockType.VirusRed ||
                    b.CurrentBlockType == Block.BlockType.VirusBlue ||
                    b.CurrentBlockType == Block.BlockType.VirusYellow)
                    continue;

                // ����̃u���b�N
                Block db = GetBlockInfo(h, v + 1);

                // �E�����J�v�Z���Ȃ�ȉ��̃`�F�b�N���s��
                if (b.CurrentBlockRota == Block.BlockRota.Right)
                {
                    // ����̃u���b�N�i�����J�v�Z���̂͂��j
                    Block lb = GetBlockInfo(h - 1, v);

                    if (lb.IsFall && (db == null || db.IsFall))
                    {
                        // ������������Ȃ�A����ɗ����悪����Ȃ痎��������
                        b.IsFall = true;
                        isFall = true;
                    }
                    else
                    {
                        // ���������������������Ȃ�
                        b.IsFall = false;
                        lb.IsFall = false;
                    }
                }
                else
                {
                    if (db == null || db.IsFall)
                    {
                        // �����悪����Ȃ痎��������
                        b.IsFall = true;
                        // �������J�v�Z���Ȃ瑊���i�E�����j�������邩������܂ŗ����������I���ɂ��Ȃ�
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
    /// �u���b�N�������o
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayFallBlock()
    {
        // �����ړ�
        for (int v = VERTICAL_NUM - 2; v >= 0; v--)
        {
            for (int h = 0; h < HORIZONTAL_NUM; h++)
            {
                Block b = GetBlockInfo(h, v);
                if (b == null || !b.IsFall) continue;

                // ��������
                b.IsFall = false;
                b.Move(Block.BlockMove.Down);

                // �u���b�N�������Ɉڍs
                blockInfo[h, v] = null;
                blockInfo[h, v + 1] = b;
            }
        }

        // �����Ԃ��J����
        float time = 0;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// �z�u���ꂽ�u���b�N�����擾
    /// </summary>
    /// <param name="x">X�ʒu</param>
    /// <param name="y">Y�ʒu</param>
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
    /// �r�̊O������
    /// </summary>
    /// <param name="x">X�ʒu</param>
    /// <param name="y">Y�ʒu</param>
    /// <returns></returns>
    private bool IsOutBottle(int x, int y)
    {
        return (x < 0 || x >= HORIZONTAL_NUM || y < 0 || y >= VERTICAL_NUM);
    }

    /// <summary>
    /// �Q�[���N���A��Ԃ�����
    /// </summary>
    /// <returns></returns>
    private bool IsGameClear()
    {
        return virusBlockList.Count == 0;
    }
}
