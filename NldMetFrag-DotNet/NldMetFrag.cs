﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NldMetFrag_DotNet.Algorithm;
using NldMetFrag_DotNet.Spectra;
using org.openscience.cdk.graph;
using org.openscience.cdk.interfaces;

namespace NldMetFrag_DotNet
{
	public class NldMetFrag
	{
		private readonly IList<IAtomContainer> candidateSdfs;

		public NldMetFrag(string candidatesSdfString)
		{
			candidateSdfs = new SdfParser().Parse(candidatesSdfString).ToList();
		}

		public IEnumerable<ResultRow> metFrag(double exactMass, string strPeaks, int mode, double mzabs, double mzppm, CancellationToken isCancelled)
		{
			var config = new FragmentationConfig(mzabs, mzppm, mode);
			var spectrum = new Spectrum(strPeaks, exactMass, mode);

			var algorithm = new FragmentationAlgorithm(
				new ImplicitHydrogenAdder(),
				new FragmentGenerator(spectrum, config),
				new HitsMatcher(spectrum, config));

			var results = candidateSdfs
				.Where(c => c != null)
				.Where(ConnectivityChecker.isConnected)
				.Select((molecule, i) => algorithm.GenerateFragments(molecule, i.ToString(CultureInfo.InvariantCulture), isCancelled))
				.Where(r => r != null)
				.ToList();

			return results;
		}
	}
}