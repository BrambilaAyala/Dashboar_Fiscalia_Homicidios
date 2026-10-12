import React, { Component } from 'react';

export class FiscaOcr extends Component {
  static displayName = FiscaOcr.name;

  constructor(props) {
    super(props);
    this.state = { pdfContent: '', loading: true };
  }

  componentDidMount() {
    this.fetchPdfContent();
  }

  async fetchPdfContent() {
    try {
      const response = await fetch('/LectorPdfOcr');
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
      const text = await response.text();
      //console.log(text);
      this.setState({ pdfContent: text, loading: false });
    } catch (error) {
      console.error('There was a problem with the fetch operation:', error);
      this.setState({ pdfContent: 'Error loading PDF content', loading: false });
    }
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : <pre>{this.state.pdfContent}</pre>;

    return (
      <div>
        <h1 id="tableLabel">Contenido del PDF de estados del pais</h1>
        {contents}
      </div>
    );
  }
}
