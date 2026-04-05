using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.Domain.Enums
{
    /* 
     * Enum representing various chronic conditions that a patient may have.
     * Using the [Flags] attribute to allow combination of multiple chronic conditions. 
     */
    [Flags]
    public enum ChronicCondition
    {
        None = 0,
        Diabetes = 1,
        Hypertension = 2,
        HeartDisease = 4,
        BleedingDisorder = 8,
        Pregnant = 16,
        AntibioticAllergy = 32,
        KidneyDisease = 64
    }
}
