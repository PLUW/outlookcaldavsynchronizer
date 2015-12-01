﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  public partial class TestResultDisplay : Form, ITestDisplay, IManualAssertUi
  {
    public TestResultDisplay ()
    {
      InitializeComponent();
      _assertLabel.Text = string.Empty;
      _testsTreeView.AfterSelect += TestsTreeView_AfterSelect;
    }

    private void TestsTreeView_AfterSelect (object sender, TreeViewEventArgs e)
    {
      _detailsTextBox.Text = (string) e.Node.Tag;
    }

    public void SetPassed (MethodInfo test)
    {
      var testName = CalculateTestName (test);
      var node = GetOrCreateNode (testName);
      node.ForeColor = Color.Green;
    }

    private static string CalculateTestName (MethodInfo test)
    {
      return string.Format ("{0}.{1}", test.DeclaringType.FullName, test.Name);
    }

    public void SetFailed (MethodInfo test, Exception x)
    {
      var testName = CalculateTestName (test);
      var node = GetOrCreateNode (testName);

      node.Tag = x.ToString();

      for (var n = node; n != null; n = n.Parent)
        n.ForeColor = Color.Red;
    }

    public void SetRunPending (MethodInfo test)
    {
      var testName = CalculateTestName (test);
      var node = GetOrCreateNode (testName);

      for (var n = node; n != null; n = n.Parent)
        n.ForeColor = Color.Black;
    }


    private Dictionary<string, TreeNode> _nodesByPath = new Dictionary<string, TreeNode>();

    private TreeNode GetOrCreateNode (string path)
    {
      TreeNode node;
      if (_nodesByPath.TryGetValue (path, out node))
      {
        return node;
      }

      if (path.Contains ('.'))
      {
        var seperator = path.LastIndexOf ('.');
        var parentPath = path.Substring (0, seperator);
        var parent = GetOrCreateNode (parentPath);
        var nodeName = path.Substring (seperator + 1, path.Length - seperator - 1);
        node = parent.Nodes.Add (nodeName);

        if (!parent.IsExpanded)
          parent.Expand();
      }
      else
      {
        node = _testsTreeView.Nodes.Add (path);
      }

      _nodesByPath.Add (path, node);

      return node;
    }

    private volatile bool _assertResult;
    private volatile bool _assertResultSet;

    public bool Assert (string instruction)
    {
      _assertLabel.Text = instruction;
      _assertResultSet = false;
      while (!_assertResultSet)
      {
        Application.DoEvents();
      }

      return _assertResult;
    }

    private void _assertFalseButton_Click (object sender, EventArgs e)
    {
      _assertResult = false;
      _assertResultSet = true;
    }

    private void _assertTrueButton_Click (object sender, EventArgs e)
    {
      _assertResult = true;
      _assertResultSet = true;
    }
  }
}