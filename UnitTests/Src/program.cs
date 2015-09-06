using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests {

	static class program {

		/////////////////////////////////////////////////////////////////////////////

		static void Main()
		{
			var test = new UnitTest1 {};
			test.ProgramInvoked = true;

			//test.InitializeTest();

			//test.RunLess();
			//test.RunLessError1();

			//test.RunNmp();
			//test.RunNmpError1();

			//test.MultipleTests1();
			//test.MultipleTests2();

			//test.Chaining1();

			//test.ReplaceTest1();

			//test.RunOnlyTest1();

			test.TypescriptTest1();
    }

	}
}
