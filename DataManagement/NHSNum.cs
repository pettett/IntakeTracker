namespace IntakeTrackerApp.DataManagement;

public static class NHSNum
{
	const int nhsModulo = 11;
	const int nhsLength = 10;
	const ulong nhsMax = 999_999_9999;
	public static bool IsValid(ulong numericNHSNum)
	{
		if (numericNHSNum > nhsMax) return false;

		//apply the very simple modulo11 checksum
		static int GetDigit(ulong num, int n) => (int)(num / (ulong)Math.Pow(10, n) % 10);

		//loop through every value except check (index 9)

		//Get digit from numeric number - will be in range  0-9

		//Sum them to later find checkup

		int sum = Enumerable.Range(0, nhsLength).Select(i => GetDigit(numericNHSNum, i) * (10 - i)).Sum();

		//If check is 10 number is always invalid, but value 11 should be 0
		int check = (nhsModulo - sum % nhsModulo) % nhsModulo;

		return GetDigit(numericNHSNum, nhsModulo - 1) == check;
	}

	public static ulong GenRandom(Random rand)
	{
		ulong n = nhsMax + 1;
		while (!IsValid(n))
		{
			n = (ulong)rand.NextInt64((long)nhsMax);
		}
		return n;
	}
}
