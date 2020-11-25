﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.CodeAnalysis.Sarif.Sarifer
{
    /// <summary>
    /// Keeps track of the set of <see cref="ITextView"/>s that are open on each tracked
    /// <see cref="ITextBuffer"/>, and notifies subscribers when the first view on a buffer
    /// is open or the last view on a buffer is closed.
    /// </summary>
    [Export(typeof(ITextBufferViewTracker))]
    public class TextBufferViewTracker : ITextBufferViewTracker
    {
        private readonly IDictionary<ITextBuffer, TextBufferViewTrackingInformation> bufferToViewsDictionary = new Dictionary<ITextBuffer, TextBufferViewTrackingInformation>();

        /// <inheritdoc/>
        public event EventHandler<FirstViewAddedEventArgs> FirstViewAdded;

        /// <inheritdoc/>
        public event EventHandler<LastViewRemovedEventArgs> LastViewRemoved;

        /// <inheritdoc/>
        public void AddTextView(ITextView textView, string path, string text)
        {
            bool first = false;

            textView = textView ?? throw new ArgumentNullException(nameof(textView));

            if (!this.bufferToViewsDictionary.TryGetValue(textView.TextBuffer, out TextBufferViewTrackingInformation trackingInformation))
            {
                first = true;
                trackingInformation = new TextBufferViewTrackingInformation();
                this.bufferToViewsDictionary.Add(textView.TextBuffer, trackingInformation);
            }

            trackingInformation.Add(textView);
            if (first)
            {
                FirstViewAdded?.Invoke(this, new FirstViewAddedEventArgs(path, text));
            }
        }

        /// <inheritdoc/>
        public void RemoveTextView(ITextView textView)
        {
            textView = textView ?? throw new ArgumentNullException(nameof(textView));

            if (!this.bufferToViewsDictionary.TryGetValue(textView.TextBuffer, out TextBufferViewTrackingInformation trackingInformation))
            {
                return;
            }

            trackingInformation.Remove(textView);
            if (trackingInformation.Views.Count == 0)
            {
                this.bufferToViewsDictionary.Remove(textView.TextBuffer);
                LastViewRemoved?.Invoke(
                    this,
                    new LastViewRemovedEventArgs(trackingInformation.LogId));
            }
        }
    }
}