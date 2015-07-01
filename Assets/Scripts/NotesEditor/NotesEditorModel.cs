﻿using System.Collections.Generic;
using System.Linq;
using UniRx;

public enum EditTypeEnum
{
    NormalNotes,
    LongNotes
}

public class NotesEditorModel : SingletonGameObject<NotesEditorModel>
{
    public ReactiveProperty<float> BPM = new ReactiveProperty<float>(0);
    public ReactiveProperty<float> Volume = new ReactiveProperty<float>(1);
    public ReactiveProperty<bool> IsPlaying = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> DivisionNumOfOneMeasure = new ReactiveProperty<int>();
    public ReactiveProperty<float> CanvasOffsetX = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasScaleFactor = new ReactiveProperty<float>();

    public ReactiveProperty<bool> WaveGraphEnabled = new ReactiveProperty<bool>(true);
    public ReactiveProperty<int> BeatOffsetSamples = new ReactiveProperty<int>(0);
    public ReactiveProperty<EditTypeEnum> EditType = new ReactiveProperty<EditTypeEnum>(EditTypeEnum.NormalNotes);
    public Subject<Block> ConfirmLongNoteStream = new Subject<Block>();
    public Subject<Block> ConfirmNormalNoteStream = new Subject<Block>();
    public Dictionary<string, Block> ShowingBlockDic = new Dictionary<string, Block>();
    public Dictionary<int, MusicModel.NoteInfo[]> NotesData = new Dictionary<int, MusicModel.NoteInfo[]>();
    public List<Block> LongNotesTempList = new List<Block>();

    void Awake()
    {
        // ロングノーツが押されたとき
        ConfirmLongNoteStream
            .Select(block => new MusicModel.NoteInfo(block.sample.Value, block.BlockNum, block.state.Value))
                .Do(noteInfo => {
                    if (noteInfo.state == 2)
                    {
                        var startBlock = LongNotesTempList.OrderBy(x => x.sample.Value).FirstOrDefault();
                        noteInfo.longNoteStartSample = startBlock.sample.Value;
                        noteInfo.longNoteStartBlockNum = startBlock.BlockNum;
                    }
                })
                .Subscribe(noteInfo => SetNote(noteInfo));

        ConfirmNormalNoteStream
            .Select(block => new MusicModel.NoteInfo(block.sample.Value, block.BlockNum, block.state.Value))
                .Subscribe(noteInfo => SetNote(noteInfo));

        EditType.Where(x => x == EditTypeEnum.NormalNotes).Subscribe(_ => LongNotesTempList.Clear());
    }

    public void SetNote(MusicModel.NoteInfo noteInfo)
    {
        if (!NotesData.ContainsKey(noteInfo.sample))
        {
            NotesData[noteInfo.sample] = new MusicModel.NoteInfo[5];
        }

        NotesData[noteInfo.sample][noteInfo.blockNum] = noteInfo;
    }

    public MusicModel.NoteInfo GetNote(int samples, int blockNum)
    {
        if (NotesData.ContainsKey(samples))
        {
            return NotesData[samples][blockNum];
        }

        return null;
    }

}
