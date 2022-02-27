namespace physicSimulator
{
    public class physics_simulator {

        public physics_simulator(Environement.Environment env, PRace.Boat boat, PRace.Time time)
        {
            this.env = env;
            this.boat = boat;
            this.time = time;
        }

    private Environement.Environment env;

        private PRace.Boat boat;

        private PRace.Time time;

        private float deltat = 0;

        private float Earthradius = 6371000F;

        private float COG = 0;

        private float SOG = 0;



        public void SetBoat(PRace.Boat boat)
        {
            this.boat = boat;
        }

        public double MathMod(double a, double b)
        {
            int integerPart = (int) (a/b);
            double mod = a - integerPart*b;
            if (a*b < 0)
            {
                mod = mod + b;
            }
            return mod;
        }

        public void Move()
        {
            Dictionary<Environement.Conditions, float> envState = env.getEnvState();
            float ws, wd, cs, cd;
            envState.TryGetValue(Environement.Conditions.CurrentSpeed, out cs);
            envState.TryGetValue(Environement.Conditions.CurrentDirection, out cd);
            envState.TryGetValue(Environement.Conditions.WindSpeed, out ws);
            envState.TryGetValue(Environement.Conditions.WindDirection, out wd);
            (float x,float y) step = nextStep(ws, wd, cs, cd);
            (SOG, COG) = CalculateAngle(step);
            if (step.x != 0 || step.y != 0)
            {
                (double teta, double phi, float cap) modif = projectionOnSphere(step);
                boat.setCap(modif.cap);
                boat.GetPosition().Update(modif.phi / (MathF.PI*2) * 360, modif.teta/ MathF.PI * 180);
            }
        }

        private (float norm, float angle) CalculateAngle((float x, float y) vect)
        {
            float angle;
            float norm = CalculateNorm(vect.x, vect.y);
            if (norm == 0){
                return (0,0);
            }
            float costw = dot(1, 0, vect.x, vect.y) / norm;
            float sintw = CrossProductNorm(1, 0, vect.x, vect.y) / norm;

            if (sintw >= 0)
            {
                angle = MathF.Acos(costw);
            }
            else
            {
                angle = MathF.Acos(costw) + MathF.PI;
            }

            return (norm, angle);
        }

        private (float x, float y) nextStep(float ws, float wd, float cs, float cd)
        {
            float dirtw, windAngle;
            (float x, float y) nextStep, currentVector, windVector;
            if (cs != 0)
            {
                currentVector = (MathF.Cos(cd / 360 * 2 * MathF.PI) * cs, MathF.Sin(cd / 360 * 2 * MathF.PI) * cs);
            }
            else
            {
                currentVector = (0, 0);
            }
            if (ws != 0)
            {
                windVector = (MathF.Cos(wd / 360 * 2 * MathF.PI) * ws, MathF.Sin(wd / 360 * 2 * MathF.PI) * ws);
            }
            else
            {

                windVector = (0, 0);
            }

            (float x, float y) trueWindVector = (windVector.x - currentVector.x, windVector.y - currentVector.y);
            float twNorm = CalculateNorm(trueWindVector.x, trueWindVector.y);

            if (twNorm == 0 || boat.GetCurrentPolaire() == null)
            {
                nextStep = (currentVector.x, currentVector.y);

            }
            else
            {
                
                float costw = dot(1, 0, trueWindVector.x, trueWindVector.y) / twNorm;
                float sintw = CrossProductNorm(1, 0, trueWindVector.x, trueWindVector.y) / twNorm;

                if (sintw >= 0)
                {
                    dirtw = MathF.Acos(costw);
                }
                else
                {
                    dirtw = MathF.Acos(costw) + MathF.PI;
                }
                dirtw = dirtw / (2 * MathF.PI) * 360;

                windAngle = (float)MathMod((boat.getCap() - dirtw), 360);
                if (windAngle == 0)
                {
                    windAngle = 180;
                }
                else if (windAngle == 180)
                {
                    windAngle = 0;
                }
                else
                {
                    windAngle = windAngle - 180;
                    windAngle = (MathF.Abs(windAngle) + 180) % 180;
                }
                float capInRad = boat.getCap()/360*2*MathF.PI;
                (float x, float y) capVector = (MathF.Cos(capInRad), MathF.Sin(capInRad)); 

                float windForce = boat.GetCurrentPolaire().getSpeed(windAngle, twNorm);
                nextStep = (currentVector.x + windForce * capVector.x , currentVector.y + windForce * capVector.y);
            }
            nextStep = (nextStep.x * time.GetTickValue()/1000, nextStep.y * time.GetTickValue()/1000);
            return nextStep;
        }

        private (double teta, double phi, float cap) projectionOnSphere((float x, float y) step)
        {

            float radius = Earthradius / time.GetAccFactorValue();
            double dphi, phi, dteta, teta;
            float stepNorm = CalculateNorm(step.x, step.y);
            dteta = dot(1, 0, step.x, step.y) / radius;
            double radiusTeta = (radius * Math.Sin(boat.GetPosition().GetLatitudeAngle()));
            if (radiusTeta == 0 || step.y == 0)
            {
                dphi = 0;
            }
            else {
                dphi = step.y / MathF.Abs(step.y) * CrossProductNorm(1, 0, step.x, step.y) / radiusTeta;
            }
            float cap = boat.getCap();
            teta = boat.GetPosition().GetLatitudeAngle() + dteta;
            if (teta != MathMod(teta, MathF.PI))
            {
                teta = MathMod(- teta, MathF.PI);
                cap = cap + 180;
                phi = MathMod( (boat.GetPosition().GetLongitudeAngle() + MathF.PI - dphi), 2 * MathF.PI);
            }
            else
            {
                phi = MathMod( (boat.GetPosition().GetLongitudeAngle() + dphi), 2 * MathF.PI);
            }
            return (teta, phi, cap);
        }

        private float dot(float xa, float ya, float xb, float yb)
        {
            return xa * xb + ya * yb;
        }

        private float CrossProductNorm(float xa, float ya, float xb, float yb)
        {
            float z = ya * xb - yb * xa;
            return (MathF.Abs(z));
        }

        private float CalculateNorm(float x, float y)
        {
            return MathF.Sqrt(x * x + y * y);
        }

        public float GetCOG()
        {
            return COG;
        }

        public float GetSOG()
        {
            return SOG;
        }
    }
}