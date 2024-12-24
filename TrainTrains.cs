using System;
using System.Security.Permissions;
using System.Security;
using BepInEx;
using MonoMod.Cil;
using Mono.Cecil.Cil;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TrainTrains
{
    [BepInPlugin("shrubfromtomorrow.trainTrains", "TrainTrains", "0.1.0")]
    public class TrainTrains : BaseUnityPlugin
    {
        private bool init;
        private static SoundID trainTrains_choochoo;
        public static int cooldown = 0;
        public bool cooldownBool = TrainTrainsRemix.cooldownBool.Value;

        public void OnEnable()
        {
            trainTrains_choochoo = new SoundID("trainTrains_choochoo", true);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            if (init) return;
            init = true;

            try
            {
                IL.Lizard.AttemptBite += Lizard_AttemptBite_Hook;
                On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate_Hook;
            }

            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
            finally
            {
                orig(self);
                MachineConnector.SetRegisteredOI("shrubfromtomorrow.trainTrains", new TrainTrainsRemix());
            }
        }

        private void RainWorldGame_RawUpdate_Hook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (cooldown > 0) { cooldown--; };
        }

        private void Lizard_AttemptBite_Hook(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                var skipCustomLabel = il.DefineLabel();
                c.GotoNext(
                    MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall(typeof(Creature).GetProperty(nameof(Lizard.grasps)).GetGetMethod()),
                    x => x.MatchLdcI4(0)
                );
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<Lizard, bool>>((Lizard zard) =>
                {
                    return (zard.Template.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TrainLizard && cooldown == 0);
                });
                c.Emit(OpCodes.Brfalse, skipCustomLabel);
                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Ldfld, typeof(UpdatableAndDeletable).GetField("room"));
                c.Emit<TrainTrains>(OpCodes.Ldsfld, "trainTrains_choochoo");
                c.EmitDelegate<Func<bool>>(() =>
                {
                    if (cooldownBool)
                    {
                        cooldown = 120;
                    }
                    return true;
                });
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Call, typeof(Creature).GetProperty("mainBodyChunk").GetGetMethod());
                c.Emit(OpCodes.Callvirt, typeof(Room).GetMethod(nameof(Room.PlaySound), new Type[] { typeof(SoundID), typeof(BodyChunk) }));
                c.Emit(OpCodes.Pop);
                c.MarkLabel(skipCustomLabel);

            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }
    }
}