using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using UnityEngine;
using WorkerSpine;
using static CreatureGenerate.CreatureGenerateData;
using static NoteEffect;
using static System.Net.Mime.MediaTypeNames;


namespace LayelossherWeapon1
{
    public class LayelossherWeapon1 : EquipmentScriptBase
    {
        //----------------------------------------------------------------------
        public override void OnStageStart()
        {
            base.OnStageStart();
            this._bufCount = 0;
            this.skillAvailable = false;
            this._LongRange = false;
            this.canGive = true;
            this.attackCounter = 1;
        }

        //----------------------------------------------------------------------
        public override EquipmentScriptBase.WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            WorkerModel workerModel = base.model.owner as WorkerModel;
            //this.GiveBarrier(workerModel);を使うための処理
            int[] array = new int[]
            {
                    4,
                    //9は遠距離攻撃が一度に何回当たるかの数値
                    3,
                    //3も近距離攻撃が一度に何回当たるかの数値
                    2
                    //
            };
            //arrayの値を設定している。「array[0]」は8、「array[1]」は3に設定されている。
            //(arrayは0から順に増えていくやつ。イメージとしては整理券みたいなの感じ)
            List<DamageInfo> list = new List<DamageInfo>();
            //List<int> list2 = new List<int>();
            string LW1_animationName = string.Empty;
            //animationNameは変更可能、アニメーションについて話し合う机の名前みたいなイメージ
            if (MovableObjectNode.GetDistance(base.model.owner.GetMovableNode(), target.GetMovableNode()) < 7f)
            {
                for (int i = 0; i < array[1]; i++)
                {
                    list.Add(base.model.metaInfo.damageInfos[i + 4].Copy());
                    //遠距離攻撃版のコメントアウトを先に見た方がいいよ
                    //この場合4から始まって4,5,6のダメージ情報を使う。
                }
                LW1_animationName = base.model.metaInfo.animationNames[1];
                this._LongRange = false;
            }
            //↑は近距離攻撃の主なコード。もし「flag」がtrueならbase.model.metaInfo.damageInfos[i + 1]の
            //ダメージ情報をコピーしてリストに追加し、アニメーションを<animation></animation>の2番目にする(0から始まる為1は2番目)
            //forは攻撃時連続で攻撃が当たるようにするための文
            //最後に敵の足を遅くするバフが近距離のバフになるようにしている。
            else
            {
                if (this._special)
                {
                    this._special = false;
                    //ist2.AddRange(LayelossherWeapon1.specialDamageAry);
                    //foreach (int LW1index in list2)
                    //{
                    //    list.Add(this.GetDamage(LW1index));
                    //}
                    //消した理由は↓のfor文の方が短かったから。ちなみにこれ関連で下のspecialDamageAryのとlist2も消されてる
                    for (int k = 0; k < array[0]; k++)
                    {
                        list.Add(base.model.metaInfo.damageInfos[7].Copy());
                    }
                    for (int j = 0; j < array[0]; j++)
                    {
                        list.Add(base.model.metaInfo.damageInfos[8].Copy());
                    }
                }
                else
                {
                    for (int l = 0; l < array[2]; l++)
                    {
                        for (int m = 0; m < array[0]; m++)
                        {
                            list.Add(base.model.metaInfo.damageInfos[m].Copy());
                            //damageInfos[j+0]にすることでforの実行ごとに0に1足されていく(forを実行するごとにjが1増える)ので
                            //damageInfos 0,1,2,3,4(jは最大4なので0+4の4まで)のダメージ情報を使うことになる。
                            //damageInfos[0]の場合、0はずっと0のままなのでdamageInfos 0のダメージ情報だけを使う。
                        }
                    }
                }
                LW1_animationName = base.model.metaInfo.animationNames[0];
                this._LongRange = true;
                //↑の遠距離版。
            }
            if (this.canGive)
            {
                this.GiveBarrier(workerModel, target);
            }
            this.attackCounter++;
            //attackCounterに1追加
            this.BarrierCoolTime();
            return new WeaponDamageInfo(LW1_animationName, list.ToArray());
        }
        //private DamageInfo GetDamage(int LW1index)
        //{
        //    return base.model.metaInfo.damageInfos[LW1index].Copy();
        //}
        //----------------------------------------------------------------------
        public override void OnAttackEnd(UnitModel actor, UnitModel target)
        {
            this._LongRange = false;
            //バグを防ぐためのコード
            if (this.attackCounter >= 4)
            {
                this._special = true;
                this.attackCounter = 0;
            }
            //4中1回special攻撃
            base.OnAttackEnd(actor, target);
        }
        //----------------------------------------------------------------------
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (target.hp <= 0f)
            {
                this.skillAvailable = true;
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }
        //バグを防ぐためのコード
        //----------------------------------------------------------------------
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            if (this.skillAvailable && this._bufCount < 10)
            //10回以上は実行できない(体力と移動速度のバフは10回しか掛けれない)
            {
                WorkerModel workerModel = actor as WorkerModel;
                UnitStatBuf unitStatBuf = new UnitStatBuf(float.MaxValue)
                {
                    duplicateType = BufDuplicateType.UNLIMIT
                };
                unitStatBuf.primaryStat.hp += 20;
                unitStatBuf.primaryStat.battle += 20;
                workerModel.AddUnitBuf(unitStatBuf);
                this._bufCount++;
                this.skillAvailable = false;
            }
            //相手のhpが0になったとき自分の体力と移動速度を上げるコード
            if (this._LongRange)
            {
                this.SlowAll(actor.GetMovableNode().currentPassage);
            }
            else
            {
                this.SlowTarget(target);
            }
            base.OnGiveDamageAfter(actor, target, dmg);
            //敵に与えるバフが近距離と遠距離、どちらのかを決定している
        }
        //敵にダメージを与えた後の処理
        //----------------------------------------------------------------------
        private void SlowTarget(UnitModel target)
        {
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new LayelossherWeapon1Buf_short());
            }
        }
        //_LongRangeがfalseで敵にダメージを与え終わったときに実行される。
        //敵にLayelossherWeapon1Buf_shortのバフを与えている
        //----------------------------------------------------------------------
        private void SlowAll(PassageObjectModel passage)
        {
            if (passage != null)
            {
                foreach (MovableObjectNode movableObjectNode in passage.GetEnteredTargets(base.model.owner.GetMovableNode()))
                {
                    UnitModel unit = movableObjectNode.GetUnit();
                    if (this.IsHostile(unit))
                    {
                        unit.AddUnitBuf(new LayelossherWeapon1Buf_long());
                    }
                }
            }
        }
        //_LongRangeがtureで敵にダメージを与え終わったときに実行される。
        //エリアに居る全ての敵にLayelossherWeapon1Buf_longのバフを与えている
        //----------------------------------------------------------------------
        private bool IsHostile(UnitModel target)
        {
            if (target.hp <= 0f)
            {
                return false;
            }
            if (!target.IsAttackTargetable())
            {
                return false;
            }
            WorkerModel workerModel = base.model.owner as WorkerModel;
            return target != workerModel && (base.model.owner.IsHostile(target) || (workerModel != null && workerModel.IsPanic()) || target is CreatureModel);
        }
        //
        //----------------------------------------------------------------------

        private void BarrierCoolTime()
        {
            if (!this.canGive && onBarrierTimer <= Time.time - 4f)
            //if:canGiveがtrueの逆(false)&バリアが張られた時刻から4秒経ったとき
            //onBarrierTimer = Time.timeが304秒に実行されたらonBarrierTimerは304になる。
            //Time.time - 4fは↑のとき300だから 304 <= 300 がtrueになるには304から4秒経たせて308にならなきゃいけない
            {
                this.canGive = true;
            }
        }
        private void GiveBarrier(WorkerModel owner, UnitModel target)
        {
            if (target.hp > 0f)
            {
                owner.AddUnitBuf(new BarrierBuf(RwbpType.A, 500f, 1000f));
            }
            //敵を倒したときにちょっと強いバリアを張るようにしている
            owner.AddUnitBuf(new BarrierBuf(RwbpType.A, 100f, 10f));
            onBarrierTimer = Time.time;
            //バリアが張られた時の時刻を取得。
            this.canGive = false;
        }
        //バリアを張るための処理。

        public object __instance;
        public FieldInfo field;

        private int _bufCount;
        private bool skillAvailable = false;
        private bool _LongRange;

        //private WorkerModel worker;

        private int attackCounter;

        private bool _special = false;
        private bool canGive = true;
        private float onBarrierTimer;

        //private bool LW1bool1 = true;

        //bool=trueかfalseしか入らない
        //int=数字のみ
        //float=数字とtrueかfalseが入ってなきゃダメ
        //class LayelossherWeapon1で使う関数を定義

        //private static int[] specialDamageAry = new int[]
        //{
        //    11,
        //    12,
        //    13,
        //    14,
        //    15,
        //    16,
        //    17,
        //    18
        //};
        //\Equipment\txts\LayelossherWeapon1.txt の<damage></damage>の何番目を使うかを
        //直接指定するやり方もあったのだが、なんかぎこちないのでやめた
    }
}
public class LayelossherWeapon1Buf_long : UnitBuf
{
    //----------------------------------------------------------------------
    public LayelossherWeapon1Buf_long()
    {
        this.remainTime = 6f;
        this.duplicateType = BufDuplicateType.UNLIMIT;
        this.type = UnitBufType.UNKNOWN;
    }
    //LayelossherWeapon1Buf_longの定義
    //----------------------------------------------------------------------
    public override void Init(UnitModel model)
    {
        base.Init(model);
        model.AddSuperArmorMax(this.superArmor);
        model.superArmorDefense = this.defense;
        if (model is CreatureModel)
        {
            CreatureModel creatureModel = model as CreatureModel;
            creatureModel.GetAnimScript().LoadSuperArmorEffect();
            this.superArmorEffect = creatureModel.GetAnimScript().GetSuperArmor();
            this.superArmorEffect.Init((float)((int)this.superArmor));
        }
        //this.slowEffect = EffectInvoker.Invoker("SlowEffect", model.GetMovableNode(), this.remainTime, false);
        //this.slowEffect.Attach();
        //this.slowEffect～の二つは敵にエフェクトをかけるためのコード
        this.remainTime = 6f;
        //バフの時間を設定
        UnitBuf unitBufByType = model.GetUnitBufByType(UnitBufType.UNKNOWN);
        //UNKNOWNのところはpublic enum UnitBufTypeにあるやつならなんでも良い。
        if (unitBufByType != null)
        {
            model.RemoveUnitBuf(unitBufByType);
        }
        if (model is CreatureModel)
        {
            this.creature = (model as CreatureModel);
            this.creature.movementScale = this.creature.movementScale * this.MovementScale();
        }
    }
    //
    //----------------------------------------------------------------------
    public override float MovementScale()
    {
        return 0.3f;
    }
    //敵の足をどのくらい遅くするかのコード。敵の元の速さ×returnの値 がバフ有りの敵の速さになる。
    //----------------------------------------------------------------------
    public override void OnUnitDie()
    {
        base.OnUnitDie();
        this.Destroy();
    }
    //
    //----------------------------------------------------------------------
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (this.creature != null)
        {
            this.creature.movementScale = this.creature.movementScale / this.MovementScale();
        }
        this.model.SubSuperArmorMax(this.superArmor);
        if (this.model is CreatureModel)
        {
            (this.model as CreatureModel).GetAnimScript().DeleteSuperArmorEffect();
        }
    }
    //
    //----------------------------------------------------------------------

    private CreatureModel creature;

    private float superArmor;
    private float defense;
    private SuperArmorEffect superArmorEffect;

    //private const string slowEffectSrc = "SlowEffect";
    //private EffectInvoker slowEffect;
    //class LayelossherWeapon1Buf_longで使う関数を定義
}
public class LayelossherWeapon1Buf_short : UnitBuf
{
    //----------------------------------------------------------------------
    public LayelossherWeapon1Buf_short()
    {
        this.remainTime = 3f;
        this.duplicateType = BufDuplicateType.UNLIMIT;
        this.type = UnitBufType.SLOW_BULLET;
    }

    //----------------------------------------------------------------------
    public override void Init(UnitModel model)
    {
        base.Init(model);
        //this.slowEffect = EffectInvoker.Invoker("SlowEffect", model.GetMovableNode(), this.remainTime, false);
        //this.slowEffect.Attach();

        this.remainTime = 3f;
        UnitBuf unitBufByType = model.GetUnitBufByType(UnitBufType.SLOW_BULLET);
        if (unitBufByType != null)
        {
            model.RemoveUnitBuf(unitBufByType);
        }
        if (model is CreatureModel)
        {
            this.creature = (model as CreatureModel);
            this.creature.movementScale = this.creature.movementScale * this.MovementScale();
        }
    }

    //----------------------------------------------------------------------
    public override float MovementScale()
    {
        return 0.5f;
    }

    //----------------------------------------------------------------------
    public override void OnUnitDie()
    {
        base.OnUnitDie();
        this.Destroy();
    }

    //----------------------------------------------------------------------
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (this.creature != null)
        {
            this.creature.movementScale = this.creature.movementScale / this.MovementScale();
        }
    }

    private CreatureModel creature;

    //private const string slowEffectSrc = "SlowEffect";
    //private EffectInvoker slowEffect;
}